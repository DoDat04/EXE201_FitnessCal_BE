using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealItemDTO.Request;
using FitnessCal.BLL.DTO.UserMealItemDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;

namespace FitnessCal.API.Controllers
{
    [Route("api/meal-items")]
    [ApiController]
    public class UserMealItemController : ControllerBase
    {
        private readonly IUserMealItemService _userMealItemService;
        private readonly ILogger<UserMealItemController> _logger;

        public UserMealItemController(IUserMealItemService userMealItemService, ILogger<UserMealItemController> logger)
        {
            _userMealItemService = userMealItemService;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<AddMealItemResponseDTO>>> AddMealItem([FromBody] AddMealItemDTO dto)
        {
            try
            {
                var result = await _userMealItemService.AddMealItemAsync(dto);

                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<AddMealItemResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in AddMealItem: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<AddMealItemResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found in AddMealItem: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<AddMealItemResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding meal item to meal log {MealLogId}", dto.MealLogId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<AddMealItemResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
    }
}
