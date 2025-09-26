using FitnessCal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.DAL.Define
{
    public interface IChatMessageRepository : IMongoGenericRepository<ChatMessage>
    {
        Task<ChatMessage?> GetByUserAndDateAsync(Guid userId, DateTime date);
        Task UpsertAsync(ChatMessage chatMessage);
    }
}
