using FitnessCal.BLL.BackgroundService.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitnessCal.BLL.BackgroundService
{
    public class MealNotificationWorker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MealNotificationWorker> _logger;
        private readonly MealNotificationSettings _settings;
        private CancellationTokenSource _cts;

        public MealNotificationWorker(
            IServiceProvider serviceProvider,
            ILogger<MealNotificationWorker> logger,
            IOptions<MealNotificationSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
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
            _logger.LogInformation("MealNotificationWorker started at: {Time}", DateTimeOffset.Now);
            _logger.LogInformation("Meal notification settings: Breakfast={BreakfastHour}, Lunch={LunchHour}, Dinner={DinnerHour}, Interval={IntervalMinutes}min, Enabled={Enabled}",
                _settings.BreakfastHour, _settings.LunchHour, _settings.DinnerHour, _settings.CheckIntervalMinutes, _settings.EnableNotifications);

            var checkInterval = TimeSpan.FromMinutes(_settings.CheckIntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMealNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in MealNotificationWorker");
                }

                await Task.Delay(checkInterval, stoppingToken);
            }

            _logger.LogInformation("MealNotificationWorker stopped at: {Time}", DateTimeOffset.Now);
        }

        private async Task ProcessMealNotificationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var mealNotificationScheduler = scope.ServiceProvider.GetRequiredService<IMealNotificationSchedulerService>();

            try
            {
                await mealNotificationScheduler.ProcessMealNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing meal notifications");
            }
        }
    }
}
