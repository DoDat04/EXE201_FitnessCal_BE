using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCal.Domain
{
    [Table ("ChatMessages")]
    [BsonIgnoreExtraElements]
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } 

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ChatDate { get; set; }

        public List<DailyMessage> DailyMessages { get; set; } = [];
    }

    public class DailyMessage
    {
        public int DailyId { get; set; } 
        public required string UserPrompt { get; set; }
        public required string AiResponse { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime PromptTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ResponseTime { get; set; }
    }
}
