using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement
{
    public class UserSubsciptionRepository : GenericRepository<UserSubscription>, IUserSubscriptionRepository
    {
        public UserSubsciptionRepository(FitnessCalContext context) : base(context)
        {
        }

        public async Task<UserSubscription?> GetActivePaidByUserAsync(Guid userId)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId
                    && s.PaymentStatus == "paid"
                    && s.EndDate >= DateTime.UtcNow.Date);
        }

        public async Task<bool> HasPendingByUserAsync(Guid userId)
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(s => s.UserId == userId && s.PaymentStatus == "pending");
        }
    }
}
