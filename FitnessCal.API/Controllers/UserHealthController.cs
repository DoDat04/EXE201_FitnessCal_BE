using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.UserHealthDTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserHealthController : ControllerBase
    {
        private readonly IUserHealthService _userHealthService;
        private readonly ILogger<UserHealthController> _logger;
        public UserHealthController(IUserHealthService userHealthService, ILogger<UserHealthController> logger)
        {
            _userHealthService = userHealthService;
            _logger = logger;
        }
        [HttpGet("get-health-info/{userId}")]
        public async Task<ActionResult<ApiResponse<HealthUserInfoDTO>>> GetHealthUserInfo(Guid userId)
        {
            try
            {
                var healthInfo = await _userHealthService.GetHealthUserInfoAsync(userId);
                if (healthInfo == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<HealthUserInfoDTO>
                    {
                        Success = false,
                        Message = "User health information not found.",
                        Data = null
                    });
                }
                return StatusCode(StatusCodes.Status200OK, new ApiResponse<HealthUserInfoDTO>
                {
                    Success = true,
                    Message = "User health information retrieved successfully.",
                    Data = healthInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user health information");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<HealthUserInfoDTO>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Data = null
                });
            }
        }
    }
}
