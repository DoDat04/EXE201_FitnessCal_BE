using FitnessCal.DAL.Context;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FitnessCalContext _context;

        public IUserRepository Users { get; }
        public IUserHealthRepository UserHealths { get; }
        public IFoodRepository Foods { get; }
        public IPredefinedDishRepository PredefinedDishes { get; }
        public IUserMealItemRepository UserMealItems { get; }
        public IUserMealLogRepository UserMealLogs { get; }

        public UnitOfWork(
            FitnessCalContext context,
            IUserRepository users,
            IUserHealthRepository userHealths,
            IFoodRepository foods,
            IPredefinedDishRepository predefinedDishes,
            IUserMealItemRepository userMealItems,
            IUserMealLogRepository userMealLogs)
        {
            _context = context;
            Users = users;
            UserHealths = userHealths;
            Foods = foods;
            PredefinedDishes = predefinedDishes;
            UserMealItems = userMealItems;
            UserMealLogs = userMealLogs;
        }

        public async Task<bool> Save()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
