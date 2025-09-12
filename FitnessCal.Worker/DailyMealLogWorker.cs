using FitnessCal.Worker.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DailyMealLogWorker : BackgroundService
{
    private readonly ILogger<DailyMealLogWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDailySchedulerService _schedulerService;

    public DailyMealLogWorker(
        ILogger<DailyMealLogWorker> logger,
        IServiceScopeFactory scopeFactory,
        IDailySchedulerService schedulerService)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _schedulerService = schedulerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyMealLogWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mealLogService = scope.ServiceProvider.GetRequiredService<IDailyMealLogGeneratorService>();

                await mealLogService.GenerateDailyMealLogsAsync();

                _logger.LogInformation("Daily meal logs generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily meal logs");
            }

            // Delay tới lần chạy tiếp theo
            await Task.Delay(_schedulerService.GetDelayUntilNextRun(), stoppingToken);
        }
    }
}
