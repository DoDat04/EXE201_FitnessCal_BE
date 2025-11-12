using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment?> GetByOrderCodeAsync(int orderCode);
        Task MarkPaidAsync(int paymentId, DateTimeOffset paidAt);
        Task MarkFailedAsync(int paymentId);
        Task<string?> GetStatusByOrderCodeAsync(int orderCode);
        Task<List<Payment>> GetAllWithDetailsAsync();
        Task<List<Payment>> GetPaidPaymentsWithDetailsAsync();
        Task CleanupExpiredPendingPaymentsAsync(int expirationMinutes = 30);
    }
}
