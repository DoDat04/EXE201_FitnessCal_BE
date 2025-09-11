using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Implement;
using FitnessCal.DAL.Define;
using FitnessCal.DAL.Implement;
using FitnessCal.Domain;
using FitnessCal.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scrutor;

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// DbContext
builder.Services.AddDbContext<FitnessCalContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// HttpClient cho các service
builder.Services.AddHttpClient();

// Scan tất cả repository và service (BLL + DAL)
builder.Services.Scan(scan => scan
    .FromAssemblies(
        typeof(IUserRepository).Assembly,
        typeof(UserService).Assembly
    )
    .AddClasses()
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// UnitOfWork register riêng
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Config các setting
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Worker
builder.Services.AddHostedService<DailyMealLogWorker>();

var host = builder.Build();
host.Run();
