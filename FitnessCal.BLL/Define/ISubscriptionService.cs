using FitnessCal.BLL.DTO.SubscriptionDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface ISubscriptionService
    {
        Task<List<UserSubscriptionResponseDTO>> GetAllUserSubscriptionsAsync();
        Task<int> CountUserSubcriptionsInPackageAsync(int packageId);
        Task<int> GetTotalSubscriptionsPaymentsAsync();
        Task<UserSubscriptionResponseDTO> GetUserSubscriptionByIdAsync(Guid userId);
        Task CheckAndUpdateExpiredSubscriptionsAsync(CancellationToken cancellationToken = default);
        Task DeleteFailedPaymentsAsync(CancellationToken cancellationToken = default);
    }
}
