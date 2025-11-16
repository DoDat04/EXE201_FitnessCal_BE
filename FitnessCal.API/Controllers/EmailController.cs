using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailService emailService, ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("test")]
        public async Task<ApiResponse<bool>> TestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var result = await _emailService.SendEmailAsync(
                    request.To,
                    request.Subject,
                    request.HtmlContent
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestEmail");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = false
                };
            }
        }

        [HttpPost("guest-send-email")]
        public async Task<ApiResponse<bool>> GuestSendEmail([FromBody] GuestEmailRequest request)
        {
            try
            {
                var result = await _emailService.GuestSendEmailAsync(
                    request.From,
                    request.Subject,
                    request.HtmlContent
                );
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GuestSendEmail");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = false
                };
            }
        }
    }
}

namespace FitnessCal.BLL.DTO.CommonDTO
{
    public class TestEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
    }

    public class GuestEmailRequest
    {
        public string From { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
    }
}
