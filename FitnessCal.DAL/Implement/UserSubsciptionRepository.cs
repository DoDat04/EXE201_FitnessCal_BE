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
            var validTime = DateTime.UtcNow.AddMinutes(-30);
            
            return await _dbSet.AsNoTracking()
                .Include(s => s.Payments)
                .AnyAsync(s => s.UserId == userId 
                    && s.PaymentStatus == "pending"
                    && s.Payments.Any(p => p.CreatedAt >= validTime && p.Status == "pending"));
        }
    }
}
