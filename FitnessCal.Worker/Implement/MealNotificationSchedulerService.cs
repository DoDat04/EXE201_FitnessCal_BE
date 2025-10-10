using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.Worker.Define;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitnessCal.Worker.Implement
{
    public class MealNotificationSchedulerService : IMealNotificationSchedulerService
    {
        private readonly IMealNotificationService _mealNotificationService;
        private readonly ILogger<MealNotificationSchedulerService> _logger;
        private readonly MealNotificationSettings _settings;

        public MealNotificationSchedulerService(
            IMealNotificationService mealNotificationService,
            ILogger<MealNotificationSchedulerService> logger,
            IOptions<MealNotificationSettings> settings)
        {
            _mealNotificationService = mealNotificationService;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task ProcessMealNotificationsAsync()
        {
            try
            {
                if (!_settings.EnableNotifications)
                {
                    _logger.LogDebug("Meal notifications are disabled");
                    return;
                }

                var currentHour = DateTime.Now.Hour;
                _logger.LogInformation("Processing meal notifications for hour: {Hour}", currentHour);

                if (!await ShouldSendNotificationAsync(currentHour))
                {
                    _logger.LogDebug("No notification needed for hour: {Hour}", currentHour);
                    return;
                }

                var mealType = await GetMealTypeForHourAsync(currentHour);
                _logger.LogInformation("Sending {MealType} notifications for hour: {Hour}", mealType, currentHour);

                switch (mealType.ToLower())
                {
                    case "breakfast":
                        await SendBreakfastNotificationsAsync();
                        break;
                    case "lunch":
                        await SendLunchNotificationsAsync();
                        break;
                    case "dinner":
                        await SendDinnerNotificationsAsync();
                        break;
                    default:
                        _logger.LogWarning("Unknown meal type: {MealType} for hour: {Hour}", mealType, currentHour);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing meal notifications");
            }
        }

        public async Task SendBreakfastNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting breakfast notification process");
                
                var result = await _mealNotificationService.SendBreakfastNotificationToAllUsersAsync();
                
                _logger.LogInformation("Breakfast notification completed. Success: {Success}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending breakfast notifications");
            }
        }

        public async Task SendLunchNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting lunch notification process");
                
                var result = await _mealNotificationService.SendLunchNotificationToAllUsersAsync();
                
                _logger.LogInformation("Lunch notification completed. Success: {Success}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending lunch notifications");
            }
        }

        public async Task SendDinnerNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting dinner notification process");
                
                var result = await _mealNotificationService.SendDinnerNotificationToAllUsersAsync();
                
                _logger.LogInformation("Dinner notification completed. Success: {Success}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending dinner notifications");
            }
        }

        public async Task<bool> ShouldSendNotificationAsync(int currentHour)
        {
            // Chỉ gửi thông báo vào các giờ cụ thể từ config
            return await Task.FromResult(currentHour == _settings.BreakfastHour || 
                                       currentHour == _settings.LunchHour || 
                                       currentHour == _settings.DinnerHour);
        }

        public async Task<string> GetMealTypeForHourAsync(int hour)
        {
            return await Task.FromResult(hour switch
            {
                var h when h == _settings.BreakfastHour => "breakfast",
                var h when h == _settings.LunchHour => "lunch",
                var h when h == _settings.DinnerHour => "dinner",
                _ => "unknown"
            });
        }
    }
}
