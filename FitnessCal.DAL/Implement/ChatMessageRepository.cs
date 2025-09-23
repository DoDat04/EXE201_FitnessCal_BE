using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using MongoDB.Driver;

namespace FitnessCal.DAL.Implement
{
    public class ChatMessageRepository : MongoGenericRepository<ChatMessage>, IChatMessageRepository
    {
        private readonly IMongoCollection<ChatMessage> _collection;

        public ChatMessageRepository(IMongoDatabase database)
            : base(database, "ChatMessages") 
        {
            _collection = database.GetCollection<ChatMessage>("ChatMessages");
        }

        public async Task<IEnumerable<ChatMessage>> GetByUserIdAsync(Guid userId)
        {
            var filter = Builders<ChatMessage>.Filter.Eq(c => c.UserId, userId);
            var messages = await _collection.Find(filter).ToListAsync();
            return messages;
        }
        public async Task<int> GetNextDailyIdAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;

            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(c => c.UserId, userId),
                Builders<ChatMessage>.Filter.Gte(c => c.PromptTime, today),
                Builders<ChatMessage>.Filter.Lt(c => c.PromptTime, today.AddDays(1))
            );

            var sort = Builders<ChatMessage>.Sort.Descending(c => c.DailyId);

            var lastMessage = await _collection
                .Find(filter)
                .Sort(sort)
                .FirstOrDefaultAsync();

            return lastMessage == null ? 1 : lastMessage.DailyId + 1;
        }

    }
}
