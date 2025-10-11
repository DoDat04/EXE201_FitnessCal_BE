using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.Worker.Define;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitnessCal.Worker
{
    public class MealNotificationWorker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MealNotificationWorker> _logger;
        private readonly MealNotificationSettings _settings;

        public MealNotificationWorker(
            IServiceProvider serviceProvider,
            ILogger<MealNotificationWorker> logger,
            IOptions<MealNotificationSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        // ⚙️ Chạy mỗi phút để kiểm tra và gửi thông báo bữa ăn
        [Function("MealNotificationWorker")]
        public async Task RunAsync(
            [TimerTrigger("0 */1 * * * *")] TimerInfo timer,
            FunctionContext context,
            CancellationToken cancellationToken)
        {
            var functionName = context.FunctionDefinition.Name;
            _logger.LogInformation("🚀 {FunctionName} triggered at: {time}", functionName, DateTimeOffset.UtcNow);

            if (!_settings.EnableNotifications)
            {
                _logger.LogInformation("ℹ️ Meal notifications are disabled. Skipping execution.");
                return;
            }

            try
            {
                await ProcessMealNotificationsAsync();
                _logger.LogInformation("✅ Meal notifications processed successfully at {time}.", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing meal notifications at {time}", DateTimeOffset.UtcNow);
            }
        }

        private async Task ProcessMealNotificationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var mealNotificationScheduler = scope.ServiceProvider.GetRequiredService<IMealNotificationSchedulerService>();

            await mealNotificationScheduler.ProcessMealNotificationsAsync();
        }
    }
}
