using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Implement;
using FitnessCal.DAL.Define;
using FitnessCal.DAL.Implement;
using FitnessCal.Domain;
using FitnessCal.Worker;
using FitnessCal.Worker.Define;
using FitnessCal.Worker.Implement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Scrutor;

var builder = Host.CreateApplicationBuilder(args);

// -------------------- Logging --------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// -------------------- DbContext --------------------
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


// -------------------- MongoDB --------------------
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoConnection");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("FitnessCalDB"); // ⚠️ Đặt tên DB thật
});

// -------------------- HttpClient + HttpContext --------------------
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// -------------------- Scan Repository & Service --------------------
builder.Services.Scan(scan => scan
    .FromAssemblies(
        typeof(IUserRepository).Assembly,   // DAL
        typeof(UserService).Assembly        // BLL
    )
    .AddClasses()
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// -------------------- UnitOfWork --------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// -------------------- Settings --------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// -------------------- Worker Services --------------------
builder.Services.AddScoped<IDailyMealLogGeneratorService, DailyMealLogGeneratorService>();
builder.Services.AddSingleton<IDailySchedulerService, DailySchedulerService>();

// OTP Cleanup Worker
builder.Services.AddHostedService<CleanupUsedOTPWorker>();

// -------------------- Hosted Services --------------------
builder.Services.AddHostedService<DailyMealLogWorker>();

// -------------------- Build & Run --------------------
var host = builder.Build();
host.Run();
