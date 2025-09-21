using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserActivityDTO.Request;
using FitnessCal.BLL.DTO.UserActivityDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserActivityController : ControllerBase
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<UserActivityController> _logger;

    public UserActivityController(IUserActivityService userActivityService, ILogger<UserActivityController> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserActivityResponseDTO>>>> GetUserActivities([FromQuery] DateOnly? date = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting user activities for user {UserId}, date: {Date}", userId, date);
            
            var userActivities = await _userActivityService.GetUserActivitiesAsync(userId, date);
            
            return Ok(new ApiResponse<List<UserActivityResponseDTO>>
            {
                Success = true,
                Message = userActivities.Any() ? "Lấy danh sách hoạt động thành công" : "Bạn chưa có hoạt động nào",
                Data = userActivities
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user activities");
            return StatusCode(500, new ApiResponse<List<UserActivityResponseDTO>>
            {
                Success = false,
                Message = "Có lỗi xảy ra khi lấy danh sách hoạt động",
                Data = null
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserActivityResponseDTO>>> AddUserActivity([FromBody] AddUserActivityRequestDTO request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Adding user activity for user {UserId}", userId);
            
            var userActivity = await _userActivityService.AddUserActivityAsync(userId, request);
            
            return Ok(new ApiResponse<UserActivityResponseDTO>
            {
                Success = true,
                Message = "Thêm hoạt động thành công",
                Data = userActivity
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid activity ID provided");
            return BadRequest(new ApiResponse<UserActivityResponseDTO>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Duplicate activity attempt");
            return Conflict(new ApiResponse<UserActivityResponseDTO>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding user activity");
            return StatusCode(500, new ApiResponse<UserActivityResponseDTO>
            {
                Success = false,
                Message = "Có lỗi xảy ra khi thêm hoạt động",
                Data = null
            });
        }
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUserActivity(int id, [FromBody] UpdateUserActivityRequestDTO request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating user activity {UserActivityId} for user {UserId}", id, userId);
            
            var result = await _userActivityService.UpdateUserActivityAsync(userId, id, request);
            
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Không tìm thấy hoạt động với ID {id} hoặc bạn không có quyền chỉnh sửa",
                    Data = false
                });
            }
            
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Cập nhật hoạt động thành công",
                Data = true
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data provided");
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message,
                Data = false
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation attempted");
            return Conflict(new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message,
                Data = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user activity");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Có lỗi xảy ra khi cập nhật hoạt động",
                Data = false
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUserActivity(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleting user activity {UserActivityId} for user {UserId}", id, userId);
            
            var result = await _userActivityService.DeleteUserActivityAsync(userId, id);
            
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Không tìm thấy hoạt động với ID {id} hoặc bạn không có quyền xóa",
                    Data = false
                });
            }
            
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Xóa hoạt động thành công",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user activity");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Có lỗi xảy ra khi xóa hoạt động",
                Data = false
            });
        }
    }

    [HttpGet("calories/{date}")]
    public async Task<ActionResult<ApiResponse<int>>> GetTotalCaloriesBurned(DateOnly date)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting total calories burned for user {UserId} on date {Date}", userId, date);
            
            var totalCalories = await _userActivityService.GetTotalCaloriesBurnedAsync(userId, date);
            
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Message = $"Tổng calories đốt cháy trong ngày {date:dd/MM/yyyy}: {totalCalories} cal",
                Data = totalCalories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting total calories burned");
            return StatusCode(500, new ApiResponse<int>
            {
                Success = false,
                Message = "Có lỗi xảy ra khi tính toán calories",
                Data = 0
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
