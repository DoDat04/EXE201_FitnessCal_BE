using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(FitnessCalContext context) : base(context)
        {
        }

        public async Task<Payment?> GetByOrderCodeAsync(int orderCode)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PayosOrderCode == orderCode);
        }

        public async Task MarkPaidAsync(int paymentId, DateTimeOffset paidAt)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (entity == null) return;
            entity.Status = "paid";
            entity.PaidAt = paidAt.UtcDateTime;
            await _context.SaveChangesAsync();
        }

        public async Task MarkFailedAsync(int paymentId)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (entity == null) return;
            entity.Status = "failed";
            await _context.SaveChangesAsync();
        }

        public async Task<string?> GetStatusByOrderCodeAsync(int orderCode)
        {
            return await _dbSet.AsNoTracking()
                .Where(p => p.PayosOrderCode == orderCode)
                .Select(p => p.Status)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Payment>> GetAllWithDetailsAsync()
        {
            return await _dbSet.AsNoTracking()
                .Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetPaidPaymentsWithDetailsAsync()
        {
            return await _dbSet.AsNoTracking()
                .Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
                .Include(p => p.User)
                .Where(p => p.PaidAt.HasValue)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task CleanupExpiredPendingPaymentsAsync(int expirationMinutes = 30)
        {
            var expirationTime = DateTime.UtcNow.AddMinutes(-expirationMinutes);
            
            var expiredPayments = await _dbSet
                .Where(p => p.Status == "pending" && p.CreatedAt < expirationTime)
                .ToListAsync();

            foreach (var payment in expiredPayments)
            {
                payment.Status = "failed";
                
                var subscription = await ((FitnessCalContext)_context).UserSubscriptions
                    .FirstOrDefaultAsync(s => s.SubscriptionId == payment.SubscriptionId);
                if (subscription != null)
                {
                    subscription.PaymentStatus = "failed";
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
