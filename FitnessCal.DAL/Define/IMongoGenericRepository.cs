using MongoDB.Driver;
using System.Linq.Expressions;

namespace FitnessCal.DAL.Define
{
    public interface IMongoGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<T?> GetByIdAsync(string id);

        Task AddAsync(T entity);

        Task UpdateAsync(string id, T entity);

        Task DeleteAsync(string id);

        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);

        Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
        Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
