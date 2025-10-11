using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Implement;
using FitnessCal.DAL.Define;
using FitnessCal.DAL.Implement;
using FitnessCal.Domain;
using FitnessCal.Worker.Define;
using FitnessCal.Worker.Implement;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Scrutor;

// ⚡ Build và chạy Azure Function host
var host = new HostBuilder()
    // 🧩 BẮT BUỘC: cấu hình Function Worker (nếu thiếu, Azure sẽ không thấy Function nào)
    .ConfigureFunctionsWorkerDefaults()

    // ⚙️ Logging config
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole(); // Cho phép xem log trên Azure Log Stream
    })

    // ⚙️ Configuration + Dependency Injection
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // ========== Database SQL (PostgreSQL) ==========
        services.AddDbContext<FitnessCalContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                npgsqlOptions.CommandTimeout(60);
            });
        });

        // ========== MongoDB ==========
        services.AddSingleton<IMongoClient>(_ =>
        {
            var mongoConnection = configuration.GetConnectionString("MongoConnection");
            return new MongoClient(mongoConnection);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("FitnessCalDB");
        });

        // ========== HttpClient ==========
        services.AddHttpClient();

        // ========== Scan Repository & Services ==========
        services.Scan(scan => scan
            .FromAssemblies(
                typeof(IUserRepository).Assembly,
                typeof(UserService).Assembly
            )
            .AddClasses()
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // ========== UnitOfWork ==========
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ========== App Settings ==========
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<PayOSSettings>(configuration.GetSection("PayOS"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<MealNotificationSettings>(configuration.GetSection("MealNotificationSettings"));

        // ========== Worker Services ==========
        services.AddScoped<IDailyMealLogGeneratorService, DailyMealLogGeneratorService>();
        services.AddSingleton<IDailySchedulerService, DailySchedulerService>();
        services.AddScoped<IMealNotificationSchedulerService, MealNotificationSchedulerService>();

        // ❌ KHÔNG cần AddHostedService trong Function App
        // vì bạn đang dùng [Function] + [TimerTrigger]
    })
    .Build();

// 🚀 Chạy host
host.Run();
