using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.ChatMessageDTO.Response
{
    public class HistoryChatResponse
    {
        public int DailyId { get; set; }
        public required string UserPrompt { get; set; }
        public required string AiResponse { get; set; }
        public DateTime PromptTime { get; set; }
        public DateTime ResponseTime { get; set; }
    }
}
