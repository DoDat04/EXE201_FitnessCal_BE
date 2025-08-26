using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserHealthDTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalorieCalculationController : ControllerBase
{
    private readonly ICalorieCalculationService _calorieCalculationService;
    private readonly ILogger<CalorieCalculationController> _logger;

    public CalorieCalculationController(
        ICalorieCalculationService calorieCalculationService,
        ILogger<CalorieCalculationController> logger)
    {
        _calorieCalculationService = calorieCalculationService;
        _logger = logger;
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateDailyCalories([FromBody] CalculateCaloriesRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Không thể xác định người dùng");
            }

            var result = await _calorieCalculationService.CalculateDailyCaloriesAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating daily calories");
            return StatusCode(500, "Có lỗi xảy ra khi tính toán calorie");
        }
    }

    [HttpGet("my-calories")]
    public async Task<IActionResult> GetMyDailyCalories()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Không thể xác định người dùng");
            }

            var result = await _calorieCalculationService.CalculateDailyCaloriesForUserAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting daily calories for current user");
            return StatusCode(500, "Có lỗi xảy ra khi lấy thông tin calorie");
        }
    }

    [HttpPut("update-health")]
    public async Task<IActionResult> UpdateUserHealth([FromBody] UpdateUserHealthRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Không thể xác định người dùng");
            }

            var result = await _calorieCalculationService.UpdateUserHealthAsync(userId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user health");
            return StatusCode(500, "Có lỗi xảy ra khi cập nhật sức khỏe");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserDailyCalories(Guid userId)
    {
        try
        {
            var result = await _calorieCalculationService.CalculateDailyCaloriesForUserAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting daily calories for user {UserId}", userId);
            return StatusCode(500, "Có lỗi xảy ra khi lấy thông tin calorie");
        }
    }

    private Guid GetCurrentUserId()
    {
        try
        {
            LogAllClaims();
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst("userId")?.Value 
                             ?? User.FindFirst("sub")?.Value 
                             ?? User.FindFirst("id")?.Value;
            }
            
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogInformation("Successfully extracted userId: {UserId}", userId);
                return userId;
            }
            
            _logger.LogWarning("Could not parse userId from claims: {UserIdClaim}", userIdClaim);
            return Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current user ID");
            return Guid.Empty;
        }
    }

    private void LogAllClaims()
    {
        try
        {
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            _logger.LogInformation("All claims in JWT token: {Claims}", string.Join(", ", claims));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging claims");
        }
    }
}
