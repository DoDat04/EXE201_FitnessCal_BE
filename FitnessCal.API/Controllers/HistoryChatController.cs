using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.ChatMessageDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [Route("api/history-message")]
    [ApiController]
    [Authorize]
    public class HistoryChatController : ControllerBase
    {
        private readonly IChatMessageService _chatMessageService;
        public HistoryChatController(IChatMessageService chatMessageService)
        {
            _chatMessageService = chatMessageService;
        }
        [HttpGet("{userId}")]
        public async Task<ApiResponse<IEnumerable<HistoryChatResponse>>> GetChatHistory(Guid userId)
        {
            try
            {
                var response = await _chatMessageService.GetChatHistory(userId);
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
