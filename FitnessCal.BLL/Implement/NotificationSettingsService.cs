using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.NotificationSettingsDTO;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly INotificationSettingsRepository _notificationSettingsRepository;
        private readonly ILogger<NotificationSettingsService> _logger;

        public NotificationSettingsService(
            INotificationSettingsRepository notificationSettingsRepository,
            ILogger<NotificationSettingsService> logger)
        {
            _notificationSettingsRepository = notificationSettingsRepository;
            _logger = logger;
        }

        public async Task<UserNotificationSettingsDTO?> GetUserSettingsAsync(Guid userId)
        {
            try
            {
                var settings = await _notificationSettingsRepository.GetByUserIdAsync(userId);
                if (settings == null)
                    return null;

                return MapToDTO(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notification settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserNotificationSettingsDTO> CreateUserSettingsAsync(Guid userId)
        {
            try
            {
                // Kiểm tra xem đã có settings chưa
                var existingSettings = await _notificationSettingsRepository.GetByUserIdAsync(userId);
                if (existingSettings != null)
                {
                    return MapToDTO(existingSettings);
                }

                var settings = new UserNotificationSettings
                {
                    UserId = userId,
                    IsNotificationEnabled = false,
                    BreakfastNotification = false,
                    LunchNotification = false,
                    DinnerNotification = false
                };

                var createdSettings = await _notificationSettingsRepository.CreateAsync(settings);
                return MapToDTO(createdSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user notification settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserNotificationSettingsDTO> UpdateUserSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request)
        {
            try
            {
                var settings = await _notificationSettingsRepository.GetByUserIdAsync(userId);
                if (settings == null)
                {
                    // Tạo mới nếu chưa có
                    settings = new UserNotificationSettings
                    {
                        UserId = userId,
                        IsNotificationEnabled = request.IsNotificationEnabled,
                        BreakfastNotification = request.IsNotificationEnabled ? request.BreakfastNotification : false,
                        LunchNotification = request.IsNotificationEnabled ? request.LunchNotification : false,
                        DinnerNotification = request.IsNotificationEnabled ? request.DinnerNotification : false
                    };
                    settings = await _notificationSettingsRepository.CreateAsync(settings);
                }
                else
                {
                    // Cập nhật existing settings
                    settings.IsNotificationEnabled = request.IsNotificationEnabled;
                    
                    // Nếu tắt thông báo tổng, tự động tắt tất cả các thông báo bữa ăn
                    if (!request.IsNotificationEnabled)
                    {
                        settings.BreakfastNotification = false;
                        settings.LunchNotification = false;
                        settings.DinnerNotification = false;
                    }
                    else
                    {
                        // Nếu bật thông báo tổng, cho phép cập nhật từng loại
                        settings.BreakfastNotification = request.BreakfastNotification;
                        settings.LunchNotification = request.LunchNotification;
                        settings.DinnerNotification = request.DinnerNotification;
                    }

                    settings = await _notificationSettingsRepository.UpdateAsync(settings);
                }

                return MapToDTO(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user notification settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteUserSettingsAsync(Guid userId)
        {
            try
            {
                var settings = await _notificationSettingsRepository.GetByUserIdAsync(userId);
                if (settings == null)
                    return false;

                return await _notificationSettingsRepository.DeleteAsync(settings.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user notification settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsNotificationEnabledAsync(Guid userId)
        {
            try
            {
                var settings = await _notificationSettingsRepository.GetByUserIdAsync(userId);
                return settings?.IsNotificationEnabled ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking notification enabled status for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsMealNotificationEnabledAsync(Guid userId, string mealType)
        {
            try
            {
                var settings = await _notificationSettingsRepository.GetByUserIdAsync(userId);
                if (settings == null || !settings.IsNotificationEnabled)
                    return false;

                return mealType.ToLower() switch
                {
                    "breakfast" => settings.BreakfastNotification,
                    "lunch" => settings.LunchNotification,
                    "dinner" => settings.DinnerNotification,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking meal notification enabled status for user {UserId}, meal {MealType}", userId, mealType);
                return false;
            }
        }

        public async Task<List<UserNotificationSettingsDTO>> GetUsersWithNotificationEnabledAsync()
        {
            try
            {
                var settings = await _notificationSettingsRepository.GetUsersWithNotificationEnabledAsync();
                return settings.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with notification enabled");
                throw;
            }
        }

        public async Task<List<UserNotificationSettingsDTO>> GetUsersWithMealNotificationAsync(string mealType)
        {
            try
            {
                var settings = await _notificationSettingsRepository.GetUsersWithMealNotificationAsync(mealType);
                return settings.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with meal notification for {MealType}", mealType);
                throw;
            }
        }

        private static UserNotificationSettingsDTO MapToDTO(UserNotificationSettings settings)
        {
            return new UserNotificationSettingsDTO
            {
                Id = settings.Id,
                UserId = settings.UserId,
                IsNotificationEnabled = settings.IsNotificationEnabled,
                BreakfastNotification = settings.BreakfastNotification,
                LunchNotification = settings.LunchNotification,
                DinnerNotification = settings.DinnerNotification,
                CreatedAt = settings.CreatedAt
            };
        }
    }
}
