using FitnessCal.BLL.DTO.NotificationSettingsDTO;

namespace FitnessCal.BLL.Define
{
    public interface INotificationSettingsService
    {
        Task<UserNotificationSettingsDTO?> GetUserSettingsAsync(Guid userId);
        Task<UserNotificationSettingsDTO> CreateUserSettingsAsync(Guid userId);
        Task<UserNotificationSettingsDTO> UpdateUserSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request);
        Task<bool> DeleteUserSettingsAsync(Guid userId);
        Task<bool> IsNotificationEnabledAsync(Guid userId);
        Task<bool> IsMealNotificationEnabledAsync(Guid userId, string mealType);
        Task<List<UserNotificationSettingsDTO>> GetUsersWithNotificationEnabledAsync();
        Task<List<UserNotificationSettingsDTO>> GetUsersWithMealNotificationAsync(string mealType);
    }
}
