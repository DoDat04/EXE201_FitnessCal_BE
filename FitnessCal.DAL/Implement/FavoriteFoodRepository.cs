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
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.Food.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, int foodId)
        {
            return await _fitnessCalContext.FavoriteFoods
                .AnyAsync(f => f.UserId == userId && f.FoodId == foodId);
        }
    }
}
