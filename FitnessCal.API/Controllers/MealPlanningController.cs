using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.MealPlanningDTO;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;

namespace FitnessCal.API.Controllers
{
    [Route("api/meal-planning")]
    [ApiController]
    public class MealPlanningController : ControllerBase
    {
        private readonly IMealPlanningService _mealPlanningService;
        private readonly ILogger<MealPlanningController> _logger;

        public MealPlanningController(
            IMealPlanningService mealPlanningService,
            ILogger<MealPlanningController> logger)
        {
            _mealPlanningService = mealPlanningService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ApiResponse<MealPlanningResponseDTO>>> GenerateMealPlan()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                   ?? User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<MealPlanningResponseDTO>
                    {
                        Success = false,
                        Message = "Invalid or missing user identity",
                        Data = null
                    });
                }

                var response = await _mealPlanningService.GenerateMealPlanAsync(userId);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<MealPlanningResponseDTO>
                {
                    Success = true,
                    Message = $"Thực đơn được tạo cho {response.DailyTarget.TotalCalories} kcal/ngày (lấy từ userHealth)",
                    Data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while generating meal plan");
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<MealPlanningResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating meal plan");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<MealPlanningResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok("Meal Planning Service is running");
        }
    }
}
