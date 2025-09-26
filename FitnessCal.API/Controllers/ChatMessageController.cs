using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.ChatMessageDTO.Request;
using FitnessCal.BLL.DTO.ChatMessageDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [Route("api/chat-message")]
    [ApiController]
    [Authorize]
    public class ChatMessageController : ControllerBase
    {
        private readonly IChatMessageService _chatMessageService;
        public ChatMessageController(IChatMessageService chatMessageService)
        {
            _chatMessageService = chatMessageService;
        }

        [HttpGet("{userId}")]
        public async Task<ApiResponse<IEnumerable<HistoryChatResponse>>> GetChatHistoryById(Guid userId, DateTime? dateTime)
        {
            try
            {
                var response = await _chatMessageService.GetChatHistoryById(userId, dateTime);
                return new ApiResponse<IEnumerable<HistoryChatResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Chat history retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<HistoryChatResponse>>
                {
                    Success = false,
                    Data = null,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
