using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IAllergyRepository : IGenericRepository<Allergy>
    {
        Task<IEnumerable<Allergy>> GetByUserIdAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId, int foodId);
    }
}
