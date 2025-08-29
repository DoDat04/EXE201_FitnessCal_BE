using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/firebase-test")]
    public class FirebaseTestController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<FirebaseTestController> _logger;

        public FirebaseTestController(IFirebaseService firebaseService, ILogger<FirebaseTestController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<bool>>> SendTestNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FcmToken))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "FCM token không được để trống",
                        Data = false
                    });
                }

                var result = await _firebaseService.SendNotificationAsync(request.FcmToken, request.Title, request.Body);
                
                if (result)
                {
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Gửi notification thành công!",
                        Data = true
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Gửi notification thất bại",
                        Data = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi notification"
                });
            }
        }

        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok("Firebase Test Controller is running!");
        }
    }

    public class SendNotificationRequest
    {
        public string FcmToken { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
