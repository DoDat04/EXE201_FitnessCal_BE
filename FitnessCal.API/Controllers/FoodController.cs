using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FoodDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCal.API.Controllers
{
    [Route("api/foods")]
    [ApiController]
    [Authorize]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;
        private readonly ILogger<FoodController> _logger;

        public FoodController(IFoodService foodService, ILogger<FoodController> logger)
        {
            _foodService = foodService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<FoodResponseDTO>>>> GetFoods([FromQuery] string? search = null)
        {
            try
            {
                var foods = await _foodService.GetFoodsAsync(search);
                
                string message;
                if (string.IsNullOrWhiteSpace(search))
                {
                    message = foods.Any() 
                        ? FoodMessage.FOOD_LIST_RETRIEVED_SUCCESS 
                        : FoodMessage.FOOD_LIST_EMPTY;
                }
                else
                {
                    message = foods.Any() 
                        ? FoodMessage.FOOD_SEARCH_SUCCESS
                        : FoodMessage.FOOD_SEARCH_NO_RESULTS;
                }

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<IEnumerable<FoodResponseDTO>>
                {
                    Success = true,
                    Message = message,
                    Data = foods
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting foods with search: {Search}", search);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<IEnumerable<FoodResponseDTO>>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
    }
}
