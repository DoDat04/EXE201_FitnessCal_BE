using FitnessCal.BLL.DTO.SubscriptionDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface ISubscriptionService
    {
        Task<List<UserSubscriptionResponseDTO>> GetAllUserSubscriptionsAsync();
    }
}
