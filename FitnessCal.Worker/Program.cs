using FitnessCal.BLL.Define;
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
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FitnessCal.BLL.DTO.CommonDTO;

var host = Host.CreateDefaultBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // ========== Firebase Admin ========== 
        var serviceAccountJson = configuration["Firebase__ServiceAccountKeyJson"];
        if (!string.IsNullOrEmpty(serviceAccountJson))
        {
            var options = new AppOptions()
            {
                Credential = GoogleCredential.FromJson(serviceAccountJson),
                ProjectId = configuration["Firebase__ProjectId"]
            };
            FirebaseApp.Create(options);
        }

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

        services.AddHttpClient();
        services.AddHttpContextAccessor();

        // ========== Scan Repository & Services ==========
        services.Scan(scan => scan
            .FromAssemblies(
                typeof(IUserRepository).Assembly,
                typeof(UserService).Assembly
            )
            .AddClasses()
            .AsImplementedInterfaces()
            .WithScopedLifetime());

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
    })
    .Build();

host.Run();
