using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IUserSubscriptionRepository : IGenericRepository<UserSubscription>
    {
        Task<UserSubscription?> GetActivePaidByUserAsync(Guid userId);
        Task<bool> HasPendingByUserAsync(Guid userId);
    }
}
