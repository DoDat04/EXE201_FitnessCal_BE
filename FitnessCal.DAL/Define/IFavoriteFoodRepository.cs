using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IFavoriteFoodRepository : IGenericRepository<FavoriteFood>
    {
        Task<IEnumerable<FavoriteFood>> GetByUserIdAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId, int? foodId, int? dishId);
    }
}
