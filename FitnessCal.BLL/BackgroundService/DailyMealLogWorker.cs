using FitnessCal.BLL.BackgroundService.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.BackgroundService
{
    public class DailyMealLogWorker
    {
        private readonly ILogger<DailyMealLogWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDailySchedulerService _schedulerService;
        private CancellationTokenSource _cts;

        public DailyMealLogWorker(
            ILogger<DailyMealLogWorker> logger,
            IServiceScopeFactory scopeFactory,
            IDailySchedulerService schedulerService)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _schedulerService = schedulerService;
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            Task.Run(async () => await RunAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 DailyMealLogWorker started at: {time}", DateTimeOffset.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mealLogService = scope.ServiceProvider.GetRequiredService<IDailyMealLogGeneratorService>();

                    await mealLogService.GenerateDailyMealLogsAsync();
                    _logger.LogInformation("✅ Daily meal logs generated successfully at {time}.", DateTimeOffset.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error generating daily meal logs at {time}", DateTimeOffset.UtcNow);
                }

                try
                {
                    var delay = _schedulerService.GetDelayUntilNextRun();
                    _logger.LogInformation("ℹ️ Next scheduled run in: {delay}", delay);

                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to calculate delay, defaulting to 1 min.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
