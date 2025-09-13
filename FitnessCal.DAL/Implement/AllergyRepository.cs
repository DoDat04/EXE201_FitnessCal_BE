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
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Food.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, int foodId)
        {
            return await _fitnessCalContext.Allergies
                .AnyAsync(a => a.UserId == userId && a.FoodId == foodId);
        }
    }
}
