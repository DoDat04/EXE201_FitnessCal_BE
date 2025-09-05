
namespace FitnessCal.DAL.Define
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IUserHealthRepository UserHealths { get; }
        IFoodRepository Foods { get; }
        IPredefinedDishRepository PredefinedDishes { get; }
        IUserMealItemRepository UserMealItems { get; }
        IUserMealLogRepository UserMealLogs { get; }
        IUserWeightLogRepository UserWeightLogs { get; }
        IPremiumPackageRepository PremiumPackages { get; }
        IPaymentRepository Payments { get; }
        IUserSubscriptionRepository UserSubscriptions { get; }
        IOTPRepository OTPs { get; }
        Task<bool> Save();
    }
}
