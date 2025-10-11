using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FitnessCal.Worker.Define;

namespace FitnessCal.Worker
{
    public class DailyMealLogWorker
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

        // ⚙️ Run every 1 minute + run immediately on startup (for testing)
        [Function("DailyMealLogWorker")]
        public async Task RunAsync(
            [TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation("🚀 DailyMealLogWorker triggered at: {time}", DateTime.UtcNow);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mealLogService = scope.ServiceProvider.GetRequiredService<IDailyMealLogGeneratorService>();

                await mealLogService.GenerateDailyMealLogsAsync();

                _logger.LogInformation("✅ Daily meal logs generated successfully at {time}.", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating daily meal logs at {time}", DateTime.UtcNow);
            }

            try
            {
                var delay = _schedulerService.GetDelayUntilNextRun();
                _logger.LogInformation("ℹ️ Next scheduled run in: {delay}", delay);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to calculate delay for next run.");
            }
        }
    }
}
