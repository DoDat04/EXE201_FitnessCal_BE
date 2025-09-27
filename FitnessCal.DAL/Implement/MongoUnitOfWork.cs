using FitnessCal.DAL.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.DAL.Implement
{
    public class MongoUnitOfWork : IMongoUnitOfWork
    {
        public IChatMessageRepository ChatMessages { get; }

        public MongoUnitOfWork(IChatMessageRepository chatMessageRepository)
        {
            ChatMessages = chatMessageRepository;
        }
    }
}
