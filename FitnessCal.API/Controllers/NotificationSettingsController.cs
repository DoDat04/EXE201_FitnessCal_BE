using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.NotificationSettingsDTO;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using System.Security.Claims;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/notification-settings")]
    [Authorize]
    public class NotificationSettingsController : ControllerBase
    {
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly ILogger<NotificationSettingsController> _logger;

        public NotificationSettingsController(
            INotificationSettingsService notificationSettingsService,
            ILogger<NotificationSettingsController> logger)
        {
            _notificationSettingsService = notificationSettingsService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy cài đặt thông báo của user hiện tại
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserNotificationSettingsDTO>>> GetUserSettings()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<UserNotificationSettingsDTO>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var settings = await _notificationSettingsService.GetUserSettingsAsync(userId);
                if (settings == null)
                {
                    // Tạo settings mặc định nếu chưa có
                    settings = await _notificationSettingsService.CreateUserSettingsAsync(userId);
                }

                return Ok(new ApiResponse<UserNotificationSettingsDTO>
                {
                    Success = true,
                    Message = "Lấy cài đặt thông báo thành công",
                    Data = settings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notification settings");
                return StatusCode(500, new ApiResponse<UserNotificationSettingsDTO>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy cài đặt thông báo"
                });
            }
        }

        /// <summary>
        /// Cập nhật cài đặt thông báo của user hiện tại
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<ApiResponse<UserNotificationSettingsDTO>>> UpdateUserSettings([FromBody] UpdateNotificationSettingsRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<UserNotificationSettingsDTO>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<UserNotificationSettingsDTO>
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var updatedSettings = await _notificationSettingsService.UpdateUserSettingsAsync(userId, request);

                return Ok(new ApiResponse<UserNotificationSettingsDTO>
                {
                    Success = true,
                    Message = "Cập nhật cài đặt thông báo thành công",
                    Data = updatedSettings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user notification settings");
                return StatusCode(500, new ApiResponse<UserNotificationSettingsDTO>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật cài đặt thông báo"
                });
            }
        }

        /// <summary>
        /// Xóa cài đặt thông báo của user hiện tại
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUserSettings()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _notificationSettingsService.DeleteUserSettingsAsync(userId);

                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "Xóa cài đặt thông báo thành công" : "Không tìm thấy cài đặt thông báo",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user notification settings");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa cài đặt thông báo"
                });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thông báo của user hiện tại
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<ApiResponse<object>>> GetNotificationStatus()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var isEnabled = await _notificationSettingsService.IsNotificationEnabledAsync(userId);
                var breakfastEnabled = await _notificationSettingsService.IsMealNotificationEnabledAsync(userId, "breakfast");
                var lunchEnabled = await _notificationSettingsService.IsMealNotificationEnabledAsync(userId, "lunch");
                var dinnerEnabled = await _notificationSettingsService.IsMealNotificationEnabledAsync(userId, "dinner");

                var status = new
                {
                    IsNotificationEnabled = isEnabled,
                    BreakfastNotification = breakfastEnabled,
                    LunchNotification = lunchEnabled,
                    DinnerNotification = dinnerEnabled
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy trạng thái thông báo thành công",
                    Data = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy trạng thái thông báo"
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}
