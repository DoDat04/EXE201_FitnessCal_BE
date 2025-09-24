using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement
{
    public class FavoriteFoodRepository : GenericRepository<FavoriteFood>, IFavoriteFoodRepository
    {
        private readonly FitnessCalContext _fitnessCalContext;

        public FavoriteFoodRepository(FitnessCalContext context) : base(context)
        {
            _fitnessCalContext = context;
        }

        public async Task<IEnumerable<FavoriteFood>> GetByUserIdAsync(Guid userId)
        {
            return await _fitnessCalContext.FavoriteFoods
                .Include(f => f.Food)
                .Include(f => f.PredefinedDish)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, int? foodId, int? dishId)
        {
            if (foodId.HasValue && !dishId.HasValue)
            {
                return await _fitnessCalContext.FavoriteFoods
                    .AnyAsync(f => f.UserId == userId && f.FoodId == foodId && f.DishId == null);
            }
            if (dishId.HasValue && !foodId.HasValue)
            {
                return await _fitnessCalContext.FavoriteFoods
                    .AnyAsync(f => f.UserId == userId && f.DishId == dishId && f.FoodId == null);
            }
            // invalid combination treated as exists to block
            return true;
        }
    }
}
