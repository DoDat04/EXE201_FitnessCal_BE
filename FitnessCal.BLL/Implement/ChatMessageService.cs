using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.ChatMessageDTO.Response;
using FitnessCal.DAL.Define;

public class ChatMessageService : IChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;

    public ChatMessageService(IChatMessageRepository chatMessageRepository)
    {
        _chatMessageRepository = chatMessageRepository;
    }
    public async Task<IEnumerable<HistoryChatResponse>> GetChatHistoryById(Guid userId, DateTime? dateTime = null)
    {
        // Nếu không truyền ngày → mặc định lấy hôm nay
        var targetDate = (dateTime ?? DateTime.UtcNow).Date;

        var chatMessage = await _chatMessageRepository.GetByUserAndDateAsync(userId, targetDate);

        if (chatMessage == null)
            return Enumerable.Empty<HistoryChatResponse>();

        // Map từng DailyMessage sang DTO
        return chatMessage.DailyMessages
            .OrderBy(m => m.DailyId)
            .Select(m => new HistoryChatResponse
            {
                DailyId = m.DailyId,
                UserPrompt = m.UserPrompt,
                AiResponse = m.AiResponse,
                PromptTime = m.PromptTime,
                ResponseTime = m.ResponseTime
            })
            .ToList();
    }
}
