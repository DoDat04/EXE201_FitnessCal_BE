using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserDevicesDTO;
using FitnessCal.BLL.DTO.CommonDTO;
using System.Security.Claims;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/user-devices")]
    [Authorize]
    public class UserDevicesController : ControllerBase
    {
        private readonly IUserDevicesService _userDevicesService;
        private readonly ILogger<UserDevicesController> _logger;

        public UserDevicesController(
            IUserDevicesService userDevicesService,
            ILogger<UserDevicesController> logger)
        {
            _userDevicesService = userDevicesService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách devices của user hiện tại
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserDevicesDTO>>>> GetUserDevices()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<List<UserDevicesDTO>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var devices = await _userDevicesService.GetUserDevicesAsync(userId);

                return Ok(new ApiResponse<List<UserDevicesDTO>>
                {
                    Success = true,
                    Message = "Lấy danh sách devices thành công",
                    Data = devices
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user devices");
                return StatusCode(500, new ApiResponse<List<UserDevicesDTO>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách devices"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách devices active của user hiện tại
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<UserDevicesDTO>>>> GetActiveUserDevices()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<List<UserDevicesDTO>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var devices = await _userDevicesService.GetActiveUserDevicesAsync(userId);

                return Ok(new ApiResponse<List<UserDevicesDTO>>
                {
                    Success = true,
                    Message = "Lấy danh sách devices active thành công",
                    Data = devices
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active user devices");
                return StatusCode(500, new ApiResponse<List<UserDevicesDTO>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách devices active"
                });
            }
        }

        /// <summary>
        /// Đăng ký device mới cho user hiện tại
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserDevicesDTO>>> RegisterDevice([FromBody] RegisterDeviceRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<UserDevicesDTO>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<UserDevicesDTO>
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var device = await _userDevicesService.RegisterDeviceAsync(userId, request);

                return Ok(new ApiResponse<UserDevicesDTO>
                {
                    Success = true,
                    Message = "Đăng ký device thành công",
                    Data = device
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device");
                return StatusCode(500, new ApiResponse<UserDevicesDTO>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đăng ký device"
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin device
        /// </summary>
        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<UserDevicesDTO>>> UpdateDevice([FromBody] UpdateDeviceRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new ApiResponse<UserDevicesDTO>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<UserDevicesDTO>
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var device = await _userDevicesService.UpdateDeviceAsync(userId, request);

                return Ok(new ApiResponse<UserDevicesDTO>
                {
                    Success = true,
                    Message = "Cập nhật device thành công",
                    Data = device
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<UserDevicesDTO>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device");
                return StatusCode(500, new ApiResponse<UserDevicesDTO>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật device"
                });
            }
        }

        /// <summary>
        /// Xóa device
        /// </summary>
        [HttpDelete("{deviceId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDevice(Guid deviceId)
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

                var result = await _userDevicesService.DeleteDeviceAsync(userId, deviceId);

                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "Xóa device thành công" : "Không tìm thấy device",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting device {DeviceId}", deviceId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa device"
                });
            }
        }

        /// <summary>
        /// Deactivate device
        /// </summary>
        [HttpPut("{deviceId}/deactivate")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateDevice(Guid deviceId)
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

                var result = await _userDevicesService.DeactivateDeviceAsync(userId, deviceId);

                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "Deactivate device thành công" : "Không tìm thấy device",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating device {DeviceId}", deviceId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi deactivate device"
                });
            }
        }

        /// <summary>
        /// Deactivate tất cả devices của user hiện tại
        /// </summary>
        [HttpPut("deactivate-all")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateAllDevices()
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

                var result = await _userDevicesService.DeactivateAllUserDevicesAsync(userId);

                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "Deactivate tất cả devices thành công" : "Không có device nào để deactivate",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating all devices for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi deactivate tất cả devices"
                });
            }
        }

        /// <summary>
        /// Kiểm tra device đã được đăng ký chưa
        /// </summary>
        [HttpPost("check-registration")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckDeviceRegistration([FromBody] CheckDeviceRegistrationRequest request)
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

                if (string.IsNullOrEmpty(request.FcmToken))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "FCM token không được để trống"
                    });
                }

                var isRegistered = await _userDevicesService.IsDeviceRegisteredAsync(userId, request.FcmToken);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Kiểm tra đăng ký device thành công",
                    Data = isRegistered
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking device registration");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi kiểm tra đăng ký device"
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

    public class CheckDeviceRegistrationRequest
    {
        public string FcmToken { get; set; } = string.Empty;
    }
}
