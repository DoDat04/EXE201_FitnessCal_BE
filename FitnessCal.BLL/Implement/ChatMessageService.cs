using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.ChatMessageDTO.Response;
using FitnessCal.DAL.Define;

public class ChatMessageService : IChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IMongoUnitOfWork _unitOfWork;

    public ChatMessageService(IChatMessageRepository chatMessageRepository, IMongoUnitOfWork unitOfWork)
    {
        _chatMessageRepository = chatMessageRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<IEnumerable<HistoryChatResponse>> GetChatHistoryById(Guid userId, DateTime? dateTime = null)
    {
        // Nếu có truyền ngày => lọc theo ngày
        if (dateTime.HasValue)
        {
            var targetDate = dateTime.Value.Date;
            var chatMessage = await _chatMessageRepository.GetByUserAndDateAsync(userId, targetDate);

            if (chatMessage == null)
                return Enumerable.Empty<HistoryChatResponse>();

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

        // ✅ Không truyền ngày => lấy tất cả
        var allChatMessages = await _unitOfWork.ChatMessages.GetAllAsync(c => c.UserId == userId);

        if (allChatMessages == null || !allChatMessages.Any())
            return Enumerable.Empty<HistoryChatResponse>();

        return allChatMessages
            .SelectMany(c => c.DailyMessages)
            .OrderBy(m => m.PromptTime) // Hoặc .OrderBy(m => m.DailyId)
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
