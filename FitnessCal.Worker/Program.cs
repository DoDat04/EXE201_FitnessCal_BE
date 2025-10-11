using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Implement;
using FitnessCal.DAL.Define;
using FitnessCal.DAL.Implement;
using FitnessCal.Domain;
using FitnessCal.Worker.Define;
using FitnessCal.Worker.Implement;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Scrutor;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults() // Bắt buộc cho Azure Function
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // -------------------- DbContext --------------------
        services.AddDbContext<FitnessCalContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    npgsqlOptions.CommandTimeout(60);
                });
        });

        // -------------------- MongoDB --------------------
        services.AddSingleton<IMongoClient>(sp =>
        {
            var connectionString = configuration.GetConnectionString("MongoConnection");
            return new MongoClient(connectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("FitnessCalDB");
        });

        // -------------------- HttpClient + HttpContext --------------------
        services.AddHttpClient();

        // -------------------- Scan Repository & Service --------------------
        services.Scan(scan => scan
            .FromAssemblies(
                typeof(IUserRepository).Assembly,
                typeof(UserService).Assembly
            )
            .AddClasses()
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // -------------------- UnitOfWork --------------------
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // -------------------- Settings --------------------
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<PayOSSettings>(configuration.GetSection("PayOS"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<MealNotificationSettings>(configuration.GetSection("MealNotificationSettings"));

        // -------------------- Worker Services --------------------
        services.AddScoped<IDailyMealLogGeneratorService, DailyMealLogGeneratorService>();
        services.AddSingleton<IDailySchedulerService, DailySchedulerService>();
        services.AddScoped<IMealNotificationSchedulerService, MealNotificationSchedulerService>();

        // ❌ KHÔNG ĐĂNG KÝ AddHostedService nữa
        // Các worker sẽ được gọi qua [Function] + [TimerTrigger]
    })
    .Build();

host.Run();
