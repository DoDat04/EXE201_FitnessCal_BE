using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.UserWeightLogDTO.Response;
using FitnessCal.BLL.Constants;

namespace FitnessCal.API.Controllers
{
    [Route("api/weight-logs")]
    [ApiController]
    [Authorize]
    public class UserWeightLogController : ControllerBase
    {
        private readonly IUserWeightLogService _userWeightLogService;
        private readonly ILogger<UserWeightLogController> _logger;

        public UserWeightLogController(IUserWeightLogService userWeightLogService, ILogger<UserWeightLogController> logger)
        {
            _userWeightLogService = userWeightLogService;
            _logger = logger;
        }

        [HttpGet("entries")]        
        public async Task<ActionResult<ApiResponse<IEnumerable<WeightLogResponseDTO>>>> GetAllEntries()
        {
            try
            {
                var userId = GetCurrentUserId();
                var logs = await _userWeightLogService.GetAllUserWeightLogsAsync(userId);
                var data = logs.Select(l => new WeightLogResponseDTO { LogDate = l.LogDate, WeightKg = l.WeightKg });

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<IEnumerable<WeightLogResponseDTO>>
                {
                    Success = true,
                    Message = data.Any() ? "Lấy danh sách cân nặng thành công" : "Chưa có lịch sử cân nặng",
                    Data = data
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized in GetAllEntries");
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<IEnumerable<WeightLogResponseDTO>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllEntries");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<IEnumerable<WeightLogResponseDTO>>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpGet("by-period")]        
        public async Task<ActionResult<ApiResponse<IEnumerable<WeightLogResponseDTO>>>> GetByPeriod([FromQuery] int months)
        {
            try
            {
                if (months != 1 && months != 6 && months != 12)
                {
                    return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<IEnumerable<WeightLogResponseDTO>>
                    {
                        Success = false,
                        Message = "Giá trị months chỉ hỗ trợ: 1, 6, 12",
                        Data = null
                    });
                }

                var userId = GetCurrentUserId();
                var logs = await _userWeightLogService.GetUserWeightLogsByPeriodAsync(userId, months);
                var data = logs.Select(l => new WeightLogResponseDTO { LogDate = l.LogDate, WeightKg = l.WeightKg });

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<IEnumerable<WeightLogResponseDTO>>
                {
                    Success = true,
                    Message = data.Any() ? "Lấy lịch sử cân nặng theo khoảng thời gian thành công" : "Không có dữ liệu trong khoảng thời gian này",
                    Data = data
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized in GetByPeriod");
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<IEnumerable<WeightLogResponseDTO>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByPeriod");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<IEnumerable<WeightLogResponseDTO>>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }
    }
}
