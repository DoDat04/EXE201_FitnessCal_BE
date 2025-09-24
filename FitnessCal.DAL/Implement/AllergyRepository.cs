using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement
{
    public class AllergyRepository : GenericRepository<Allergy>, IAllergyRepository
    {
        private readonly FitnessCalContext _fitnessCalContext;

        public AllergyRepository(FitnessCalContext context) : base(context)
        {
            _fitnessCalContext = context;
        }

        public async Task<IEnumerable<Allergy>> GetByUserIdAsync(Guid userId)
        {
            return await _fitnessCalContext.Allergies
                .Include(a => a.Food)
                .Include(a => a.Dish)
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Food != null ? a.Food.Name : a.Dish!.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, int? foodId, int? dishId)
        {
            if (foodId.HasValue && !dishId.HasValue)
            {
                return await _fitnessCalContext.Allergies
                    .AnyAsync(a => a.UserId == userId && a.FoodId == foodId && a.DishId == null);
            }
            if (dishId.HasValue && !foodId.HasValue)
            {
                return await _fitnessCalContext.Allergies
                    .AnyAsync(a => a.UserId == userId && a.DishId == dishId && a.FoodId == null);
            }
            return true;
        }
    }
}
