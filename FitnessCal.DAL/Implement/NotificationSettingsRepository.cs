using Microsoft.EntityFrameworkCore;
using FitnessCal.Domain;
using FitnessCal.DAL.Define;

namespace FitnessCal.DAL.Implement
{
    public class NotificationSettingsRepository : INotificationSettingsRepository
    {
        private readonly FitnessCalContext _context;

        public NotificationSettingsRepository(FitnessCalContext context)
        {
            _context = context;
        }

        public async Task<UserNotificationSettings?> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserNotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<UserNotificationSettings> CreateAsync(UserNotificationSettings settings)
        {
            settings.CreatedAt = DateTime.UtcNow;
            _context.UserNotificationSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<UserNotificationSettings> UpdateAsync(UserNotificationSettings settings)
        {
            _context.UserNotificationSettings.Update(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var settings = await _context.UserNotificationSettings.FindAsync(id);
            if (settings == null)
                return false;

            _context.UserNotificationSettings.Remove(settings);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid userId)
        {
            return await _context.UserNotificationSettings
                .AnyAsync(s => s.UserId == userId);
        }

        public async Task<List<UserNotificationSettings>> GetUsersWithNotificationEnabledAsync()
        {
            return await _context.UserNotificationSettings
                .Where(s => s.IsNotificationEnabled)
                .ToListAsync();
        }

        public async Task<List<UserNotificationSettings>> GetUsersWithMealNotificationAsync(string mealType)
        {
            return await _context.UserNotificationSettings
                .Where(s => s.IsNotificationEnabled && 
                           (mealType.ToLower() == "breakfast" && s.BreakfastNotification ||
                            mealType.ToLower() == "lunch" && s.LunchNotification ||
                            mealType.ToLower() == "dinner" && s.DinnerNotification))
                .ToListAsync();
        }
    }
}
