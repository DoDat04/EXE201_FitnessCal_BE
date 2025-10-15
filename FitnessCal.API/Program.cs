using FitnessCal.DAL.Define;
using FitnessCal.DAL;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.BackgroundService; // DailyMealLogWorker
using Microsoft.EntityFrameworkCore;
using FitnessCal.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Transformer;
using MongoDB.Driver;
using Microsoft.AspNetCore.HttpOverrides;
using FitnessCal.BLL.BackgroundService.Define;
using FitnessCal.BLL.BackgroundService.Implement;

var builder = WebApplication.CreateBuilder(args);

// ========== MongoDB ==========
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(mongoConnectionString));

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

// ========== PostgreSQL ==========
builder.Services.AddDbContext<FitnessCalContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            // 👉 Tự retry khi gặp lỗi mạng hoặc Supabase sleep
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null
            );

            // 👉 Cho phép lệnh chạy tối đa 60 giây (mặc định chỉ 15 giây)
            npgsqlOptions.CommandTimeout(60);
        });
});

builder.Services.AddMemoryCache();

// ========== CORS ==========
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
     .AllowAnyHeader()
     .AllowAnyMethod()
));

// ========== DI Repositories & Services ==========
builder.Services.Scan(scan => scan
    .FromAssemblies(
        typeof(IAuthService).Assembly,
        typeof(IUserRepository).Assembly
    )
    .AddClasses()
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Scan Transformers
builder.Services.Scan(scan => scan
    .FromAssemblyOf<TransformQueries>()
    .AddClasses(classes => classes.InNamespaceOf<TransformQueries>())
    .AsSelf()
    .WithScopedLifetime()
);

// AppSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<MealNotificationSettings>(builder.Configuration.GetSection("MealNotificationSettings"));

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// ========== Authentication JWT ==========
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:AccessSecretKey"]!;
        var issuer = builder.Configuration["Jwt:Issuer"]!;
        var audience = builder.Configuration["Jwt:Audience"]!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,
            ValidateTokenReplay = false
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authorization = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authorization))
                {
                    string token = authorization.StartsWith("Bearer ")
                        ? authorization["Bearer ".Length..]
                        : authorization;
                    if (!string.IsNullOrEmpty(token))
                        context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();

// ========== Swagger ==========
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FitnessCal API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========== Worker Service ==========
builder.Services.AddSingleton<ChangePaymentStatusWorker>();
builder.Services.AddSingleton<DailyMealLogWorker>();
builder.Services.AddSingleton<MealNotificationWorker>(); // ✅ thêm dòng này
builder.Services.AddSingleton<CleanupUsedOTPWorker>();
builder.Services.AddScoped<IChangePaymentStatusGeneratorService, ChangePaymentStatusGeneratorService>();
builder.Services.AddScoped<IDailyMealLogGeneratorService, DailyMealLogGeneratorService>();
builder.Services.AddSingleton<IDailySchedulerService, DailySchedulerService>();
var app = builder.Build();

// ========== Swagger ==========
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FitnessCal API v1");
});

// ========== HTTPS / ForwardedHeaders ==========
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

// ========== Middleware ==========
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ⚡ Khởi chạy DailyMealLogWorker khi app start
var worker = app.Services.GetRequiredService<DailyMealLogWorker>();
var worker1= app.Services.GetRequiredService<MealNotificationWorker>();
var worker2= app.Services.GetRequiredService<ChangePaymentStatusWorker>();
worker.Start();
worker1?.Start();
worker2?.Start();
app.Run();
