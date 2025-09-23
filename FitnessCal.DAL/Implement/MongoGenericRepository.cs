using FitnessCal.DAL.Define;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace FitnessCal.DAL.Implement
{
    public class MongoGenericRepository<T> : IMongoGenericRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoGenericRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter == null)
                return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).ToListAsync();
        }
        public async Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
            return await _collection.CountDocumentsAsync(predicate);
        }
    }
}
