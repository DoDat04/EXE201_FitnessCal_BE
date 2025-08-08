using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealLogDTO.Request;
using FitnessCal.BLL.DTO.UserMealLogDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCal.API.Controllers
{
    [Route("api/meal-logs")]
    [ApiController]
    [Authorize]
    public class UserMealLogController : ControllerBase
    {
        private readonly IUserMealLogService _userMealLogService;
        private readonly ILogger<UserMealLogController> _logger;

        public UserMealLogController(IUserMealLogService userMealLogService, ILogger<UserMealLogController> logger)
        {
            _userMealLogService = userMealLogService;
            _logger = logger;
        }

        [HttpPost("auto-create")]
        public async Task<ActionResult<ApiResponse<CreateUserMealLogResponseDTO>>> AutoCreateMealLogs([FromBody] CreateUserMealLogDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userMealLogService.AutoCreateMealLogsAsync(userId, dto);

                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<CreateUserMealLogResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in AutoCreateMealLogs: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<CreateUserMealLogResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in AutoCreateMealLogs: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<CreateUserMealLogResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found in AutoCreateMealLogs: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<CreateUserMealLogResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while auto-creating meal logs for user on {MealDate}", dto.MealDate);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<CreateUserMealLogResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpGet("by-date")]        
        public async Task<ActionResult<ApiResponse<GetMealLogsByDateResponseDTO>>> GetMealLogsByDate([FromQuery] DateOnly date)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userMealLogService.GetMealLogsByDateAsync(userId, date);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<GetMealLogsByDateResponseDTO>
                {
                    Success = true,
                    Message = "Lấy thông tin bữa ăn thành công",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in GetMealLogsByDate: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<GetMealLogsByDateResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found in GetMealLogsByDate: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<GetMealLogsByDateResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting meal logs for user on {Date}", date);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<GetMealLogsByDateResponseDTO>
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
