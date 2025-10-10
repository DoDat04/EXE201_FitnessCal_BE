using FitnessCal.BLL.Define;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class MealNotificationService : IMealNotificationService
    {
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly IUserDevicesService _userDevicesService;
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<MealNotificationService> _logger;

        public MealNotificationService(
            INotificationSettingsService notificationSettingsService,
            IUserDevicesService userDevicesService,
            IFirebaseService firebaseService,
            ILogger<MealNotificationService> logger)
        {
            _notificationSettingsService = notificationSettingsService;
            _userDevicesService = userDevicesService;
            _firebaseService = firebaseService;
            _logger = logger;
        }

        public async Task<bool> SendBreakfastNotificationToAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Starting breakfast notification process");
                
                // Lấy users có bật thông báo tổng VÀ bữa sáng
                var users = await _notificationSettingsService.GetUsersWithMealNotificationAsync("breakfast");
                if (!users.Any())
                {
                    _logger.LogInformation("No users have breakfast notification enabled");
                    return true;
                }

                // Lọc thêm: chỉ lấy users có IsNotificationEnabled = true
                var eligibleUsers = users.Where(u => u.IsNotificationEnabled).ToList();
                if (!eligibleUsers.Any())
                {
                    _logger.LogInformation("No users have overall notification enabled for breakfast");
                    return true;
                }

                var userIds = eligibleUsers.Select(u => u.UserId).ToList();
                var fcmTokens = await _userDevicesService.GetActiveFcmTokensByUserIdsAsync(userIds);
                
                if (!fcmTokens.Any())
                {
                    _logger.LogInformation("No active FCM tokens found for breakfast notification");
                    return true;
                }

                var (title, body) = GetMealNotificationContent("breakfast");
                var successCount = 0;

                foreach (var token in fcmTokens)
                {
                    try
                    {
                        var result = await _firebaseService.SendNotificationAsync(token, title, body);
                        if (result) successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending breakfast notification to token {Token}", token);
                    }
                }

                _logger.LogInformation("Breakfast notification completed. Success: {SuccessCount}/{TotalTokens}", 
                    successCount, fcmTokens.Count);
                
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending breakfast notifications");
                return false;
            }
        }

        public async Task<bool> SendLunchNotificationToAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Starting lunch notification process");
                
                // Lấy users có bật thông báo tổng VÀ bữa trưa
                var users = await _notificationSettingsService.GetUsersWithMealNotificationAsync("lunch");
                if (!users.Any())
                {
                    _logger.LogInformation("No users have lunch notification enabled");
                    return true;
                }

                // Lọc thêm: chỉ lấy users có IsNotificationEnabled = true
                var eligibleUsers = users.Where(u => u.IsNotificationEnabled).ToList();
                if (!eligibleUsers.Any())
                {
                    _logger.LogInformation("No users have overall notification enabled for lunch");
                    return true;
                }

                var userIds = eligibleUsers.Select(u => u.UserId).ToList();
                var fcmTokens = await _userDevicesService.GetActiveFcmTokensByUserIdsAsync(userIds);
                
                if (!fcmTokens.Any())
                {
                    _logger.LogInformation("No active FCM tokens found for lunch notification");
                    return true;
                }

                var (title, body) = GetMealNotificationContent("lunch");
                var successCount = 0;

                foreach (var token in fcmTokens)
                {
                    try
                    {
                        var result = await _firebaseService.SendNotificationAsync(token, title, body);
                        if (result) successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending lunch notification to token {Token}", token);
                    }
                }

                _logger.LogInformation("Lunch notification completed. Success: {SuccessCount}/{TotalTokens}", 
                    successCount, fcmTokens.Count);
                
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending lunch notifications");
                return false;
            }
        }

        public async Task<bool> SendDinnerNotificationToAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Starting dinner notification process");
                
                // Lấy users có bật thông báo tổng VÀ bữa tối
                var users = await _notificationSettingsService.GetUsersWithMealNotificationAsync("dinner");
                if (!users.Any())
                {
                    _logger.LogInformation("No users have dinner notification enabled");
                    return true;
                }

                // Lọc thêm: chỉ lấy users có IsNotificationEnabled = true
                var eligibleUsers = users.Where(u => u.IsNotificationEnabled).ToList();
                if (!eligibleUsers.Any())
                {
                    _logger.LogInformation("No users have overall notification enabled for dinner");
                    return true;
                }

                var userIds = eligibleUsers.Select(u => u.UserId).ToList();
                var fcmTokens = await _userDevicesService.GetActiveFcmTokensByUserIdsAsync(userIds);
                
                if (!fcmTokens.Any())
                {
                    _logger.LogInformation("No active FCM tokens found for dinner notification");
                    return true;
                }

                var (title, body) = GetMealNotificationContent("dinner");
                var successCount = 0;

                foreach (var token in fcmTokens)
                {
                    try
                    {
                        var result = await _firebaseService.SendNotificationAsync(token, title, body);
                        if (result) successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending dinner notification to token {Token}", token);
                    }
                }

                _logger.LogInformation("Dinner notification completed. Success: {SuccessCount}/{TotalTokens}", 
                    successCount, fcmTokens.Count);
                
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending dinner notifications");
                return false;
            }
        }

        private static (string title, string body) GetMealNotificationContent(string mealType)
        {
            return mealType.ToLower() switch
            {
                "breakfast" => ("Bữa sáng", "Đã đến giờ ăn sáng rồi! Hãy bắt đầu ngày mới với bữa sáng lành mạnh."),
                "lunch" => ("Bữa trưa", "Giờ ăn trưa đến rồi! Đừng quên bổ sung năng lượng cho buổi chiều."),
                "dinner" => ("Bữa tối", "Đến giờ ăn tối! Hãy thưởng thức bữa tối ngon miệng."),
                _ => ("Thông báo bữa ăn", "Đã đến giờ ăn!")
            };
        }
    }
}