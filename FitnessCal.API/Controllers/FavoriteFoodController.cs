using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FavoriteFoodDTO.Request;
using FitnessCal.BLL.DTO.FavoriteFoodDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCal.API.Controllers
{
    [Route("api/favorite-foods")]
    [ApiController]
    [Authorize]
    public class FavoriteFoodController : ControllerBase
    {
        private readonly IFavoriteFoodService _favoriteFoodService;
        private readonly ILogger<FavoriteFoodController> _logger;

        public FavoriteFoodController(IFavoriteFoodService favoriteFoodService, ILogger<FavoriteFoodController> logger)
        {
            _favoriteFoodService = favoriteFoodService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateFavoriteFoodResponseDTO>>> CreateFavoriteFood([FromBody] CreateFavoriteFoodDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _favoriteFoodService.CreateFavoriteFoodAsync(userId, dto);

                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<CreateFavoriteFoodResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in CreateFavoriteFood: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<CreateFavoriteFoodResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating favorite food");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<CreateFavoriteFoodResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<FavoriteFoodResponseDTO>>>> GetUserFavoriteFoods()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _favoriteFoodService.GetUserFavoriteFoodsAsync(userId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<IEnumerable<FavoriteFoodResponseDTO>>
                {
                    Success = true,
                    Message = "Favorite foods retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user favorite foods");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<IEnumerable<FavoriteFoodResponseDTO>>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpDelete("{favoriteFoodId}")]
        public async Task<ActionResult<ApiResponse<DeleteFavoriteFoodResponseDTO>>> DeleteFavoriteFood(int favoriteFoodId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _favoriteFoodService.DeleteFavoriteFoodAsync(favoriteFoodId, userId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<DeleteFavoriteFoodResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Favorite food not found in DeleteFavoriteFood: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<DeleteFavoriteFoodResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in DeleteFavoriteFood: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.FORBIDDEN, new ApiResponse<DeleteFavoriteFoodResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting favorite food {FavoriteFoodId}", favoriteFoodId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<DeleteFavoriteFoodResponseDTO>
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
