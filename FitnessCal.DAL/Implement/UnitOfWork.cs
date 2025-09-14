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
        public IUserWeightLogRepository UserWeightLogs { get; }
        public IPremiumPackageRepository PremiumPackages { get; }
        public IPaymentRepository Payments { get; }
        public IUserSubscriptionRepository UserSubscriptions { get; }
        public IOTPRepository OTPs { get; }
        public IAllergyRepository Allergies { get; }
        public IFavoriteFoodRepository FavoriteFoods { get; }
        public UnitOfWork(
            FitnessCalContext context,
            IUserRepository users,
            IUserHealthRepository userHealths,
            IFoodRepository foods,
            IPredefinedDishRepository predefinedDishes,
            IUserMealItemRepository userMealItems,
            IUserMealLogRepository userMealLogs,
            IUserWeightLogRepository userWeightLogs,
            IPremiumPackageRepository premiumPackages,
            IPaymentRepository payments,
            IUserSubscriptionRepository userSubscriptions,
            IOTPRepository otps,
            IAllergyRepository allergies,
            IFavoriteFoodRepository favoriteFoods)
        {
            _context = context;
            Users = users;
            UserHealths = userHealths;
            Foods = foods;
            PredefinedDishes = predefinedDishes;
            UserMealItems = userMealItems;
            UserMealLogs = userMealLogs;
            UserWeightLogs = userWeightLogs;
            PremiumPackages = premiumPackages;
            Payments = payments;
            UserSubscriptions = userSubscriptions;
            OTPs = otps;
            Allergies = allergies;
            FavoriteFoods = favoriteFoods;
        }

        public async Task<bool> Save()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
