using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealItemDTO.Request;
using FitnessCal.BLL.DTO.UserMealItemDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCal.API.Controllers
{
    [Route("api/meal-items")]
    [ApiController]
    [Authorize]
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

        [HttpDelete("{itemId}")]
        public async Task<ActionResult<ApiResponse<DeleteMealItemResponseDTO>>> DeleteMealItem(int itemId)
        {
            try
            {
                var result = await _userMealItemService.DeleteMealItemAsync(itemId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<DeleteMealItemResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in DeleteMealItem: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<DeleteMealItemResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found in DeleteMealItem: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<DeleteMealItemResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting meal item {ItemId}", itemId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<DeleteMealItemResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
    }
}
