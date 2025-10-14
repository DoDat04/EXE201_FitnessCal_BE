using FitnessCal.BLL.Constants;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.UserCapturedFoodDTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static FitnessCal.BLL.Implement.FoodService;

namespace FitnessCal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserCapturedFoodController : ControllerBase
    {
        private readonly IUserCapturedFoodService _userCapturedFoodService;
        public UserCapturedFoodController(IUserCapturedFoodService userCapturedFoodService)
        {
            _userCapturedFoodService = userCapturedFoodService;
        }

        [HttpPost("confirm-meals-detected")]
        [Authorize]
        public async Task<IActionResult> ConfirmCapturedFood([FromBody] ConfirmCapturedFoodRequest request)
        {
            var foodInfo = new ParsedFoodInfo
            {
                Name = request.Name,
                Calories = request.Calories,
                Carbs = request.Carbs,
                Fat = request.Fat,
                Protein = request.Protein
            };

            try
            {
                var result = await _userCapturedFoodService.ConfirmCapturedFood(foodInfo, request.ImageUrl);

                if (result.Success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = result.Message,
                        Data = result.Data  
                    });
                }

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR
                });
            }
        }
    }
}
