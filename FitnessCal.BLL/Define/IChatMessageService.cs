using FitnessCal.BLL.DTO.ChatMessageDTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Define
{
    public interface IChatMessageService
    {
        Task<IEnumerable<HistoryChatResponse>> GetChatHistoryById(Guid userId, DateTime? dateTime);
    }
}
