using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealLogDTO.Request;
using FitnessCal.BLL.DTO.UserMealLogDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;

namespace FitnessCal.API.Controllers
{
    [Route("api/meal-logs")]
    [ApiController]
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
                var result = await _userMealLogService.AutoCreateMealLogsAsync(dto);

                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<CreateUserMealLogResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
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
                _logger.LogError(ex, "Error occurred while auto-creating meal logs for user {UserId} on {MealDate}", 
                    dto.UserId, dto.MealDate);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<CreateUserMealLogResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpGet("by-date")]
        public async Task<ActionResult<ApiResponse<GetMealLogsByDateResponseDTO>>> GetMealLogsByDate(
            [FromQuery] Guid userId, 
            [FromQuery] DateOnly date)
        {
            try
            {
                var result = await _userMealLogService.GetMealLogsByDateAsync(userId, date);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<GetMealLogsByDateResponseDTO>
                {
                    Success = true,
                    Message = "Lấy thông tin bữa ăn thành công",
                    Data = result
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
                _logger.LogError(ex, "Error occurred while getting meal logs for user {UserId} on {Date}", userId, date);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<GetMealLogsByDateResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
    }
}
