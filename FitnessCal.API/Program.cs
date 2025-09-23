using FitnessCal.DAL.Define;
using FitnessCal.DAL;
using FitnessCal.BLL.Define;
using Microsoft.EntityFrameworkCore;
using FitnessCal.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using FitnessCal.BLL.DTO.CommonDTO;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(mongoConnectionString));

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

builder.Services.AddDbContext<FitnessCalContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMemoryCache();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
     .AllowAnyHeader()
     .AllowAnyMethod()
));

builder.Services.Scan(scan => scan
    .FromAssemblies(
        typeof(IAuthService).Assembly,    
        typeof(IUserRepository).Assembly   
    )
    .AddClasses()
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

builder.Services.AddHttpClient();

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
                    string token;
                    if (authorization.StartsWith("Bearer "))
                    {
                        token = authorization.Substring("Bearer ".Length);
                    }
                    else
                    {
                        token = authorization;
                    }
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
