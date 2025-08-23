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

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<SearchFoodPaginationResponseDTO>>> SearchFoods([FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
        {
            try
            {
                var searchResults = await _foodService.SearchFoodsAsync(search, page, pageSize);
                
                string message;
                if (string.IsNullOrWhiteSpace(search))
                {
                    message = searchResults.Foods.Any() 
                        ? $"Hiển thị trang {page}/{searchResults.TotalPages} với {searchResults.Foods.Count()} món ăn phổ biến" 
                        : "Không có món ăn nào trong database";
                }
                else
                {
                    message = searchResults.Foods.Any() 
                        ? $"Tìm thấy {searchResults.TotalCount} món ăn với từ khóa '{search}'. Trang {page}/{searchResults.TotalPages}"
                        : $"Không tìm thấy món ăn nào với từ khóa '{search}'";
                }

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<SearchFoodPaginationResponseDTO>
                {
                    Success = true,
                    Message = message,
                    Data = searchResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching foods");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<SearchFoodPaginationResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
    }
}
