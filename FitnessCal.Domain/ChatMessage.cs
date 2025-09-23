using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.Domain
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        public int DailyId { get; set; }

        public required string UserPrompt { get; set; }
        public required string AiResponse { get; set; }

        public DateTime PromptTime { get; set; }
        public DateTime ResponseTime { get; set; }
    }
}
