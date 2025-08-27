using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement
{
    public class UserWeightLogRepository : GenericRepository<UserWeightLog>, IUserWeightLogRepository
    {
        private readonly FitnessCalContext _fitnessCalContext;
        public UserWeightLogRepository(FitnessCalContext context) : base(context)
        {
            _fitnessCalContext = context;
        }

        public async Task<IEnumerable<UserWeightLog>> GetUserWeightLogsByUserIdAsync(Guid userId)
        {
            return await _fitnessCalContext.UserWeightLogs
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.LogDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserWeightLog>> GetUserWeightLogsByPeriodAsync(Guid userId, int months)
        {
            var startDate = DateTime.Now.AddMonths(-months);
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var currentDateOnly = DateOnly.FromDateTime(DateTime.Now);

            return await _fitnessCalContext.UserWeightLogs
                .Where(w => w.UserId == userId && 
                           w.LogDate >= startDateOnly && 
                           w.LogDate <= currentDateOnly)
                .OrderBy(w => w.LogDate)
                .ToListAsync();
        }

        public async Task<UserWeightLog?> GetByUserAndDateAsync(Guid userId, DateOnly logDate)
        {
            return await _fitnessCalContext.UserWeightLogs
                .FirstOrDefaultAsync(w => w.UserId == userId && w.LogDate == logDate);
        }
    }
}
