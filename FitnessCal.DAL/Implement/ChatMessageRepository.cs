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

        public async Task<ChatMessage?> GetByUserAndDateAsync(Guid userId, DateTime date)
        {
            var startOfDayUtc = date.Date.ToUniversalTime();
            var endOfDayUtc = startOfDayUtc.AddDays(1);

            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.UserId, userId),
                Builders<ChatMessage>.Filter.Gte(x => x.ChatDate, startOfDayUtc),
                Builders<ChatMessage>.Filter.Lt(x => x.ChatDate, endOfDayUtc)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }


        public async Task UpsertAsync(ChatMessage chatMessage)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(x => x.UserId, chatMessage.UserId),
                Builders<ChatMessage>.Filter.Eq(x => x.ChatDate, chatMessage.ChatDate)
            );

            await _collection.ReplaceOneAsync(
                filter,
                chatMessage,
                new ReplaceOptions { IsUpsert = true }
            );
        }

    }
}
