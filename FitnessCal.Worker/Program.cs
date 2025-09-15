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
using Scrutor;

var builder = Host.CreateApplicationBuilder(args);

// -------------------- Logging --------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// -------------------- DbContext --------------------
builder.Services.AddDbContext<FitnessCalContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// -------------------- HttpClient --------------------
builder.Services.AddHttpClient();

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
