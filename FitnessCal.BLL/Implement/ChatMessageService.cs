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

    public async Task<IEnumerable<HistoryChatResponse>> GetChatHistory(Guid userId)
    {
        var chatMessages = await _chatMessageRepository.GetByUserIdAsync(userId);

        return chatMessages
            .OrderBy(c => c.PromptTime)
            .Select(c => new HistoryChatResponse
            {
                Id = c.Id,
                UserId = c.UserId,
                DailyId = c.DailyId,
                UserPrompt = c.UserPrompt,
                AiResponse = c.AiResponse,
                PromptTime = c.PromptTime,
                ResponseTime = c.ResponseTime
            })
            .ToList();
    }
}
