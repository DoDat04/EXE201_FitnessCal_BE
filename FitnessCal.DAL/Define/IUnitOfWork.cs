
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
        Task<bool> Save();
    }
}
