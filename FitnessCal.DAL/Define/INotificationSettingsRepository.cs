using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface INotificationSettingsRepository
    {
        Task<UserNotificationSettings?> GetByUserIdAsync(Guid userId);
        Task<UserNotificationSettings> CreateAsync(UserNotificationSettings settings);
        Task<UserNotificationSettings> UpdateAsync(UserNotificationSettings settings);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid userId);
        Task<List<UserNotificationSettings>> GetUsersWithNotificationEnabledAsync();
        Task<List<UserNotificationSettings>> GetUsersWithMealNotificationAsync(string mealType);
    }
}
