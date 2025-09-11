using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.DashboardDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IUserService userService, ILogger<DashboardController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("user-statistics")]
        public async Task<ActionResult<ApiResponse<UserStatisticsDTO>>> GetUserStatistics()
        {
            try
            {
                var statistics = await _userService.GetUserStatisticsAsync();
                
                var response = new ApiResponse<UserStatisticsDTO>
                {
                    Success = true,
                    Message = "User statistics retrieved successfully",
                    Data = statistics
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user statistics");
                
                var response = new ApiResponse<UserStatisticsDTO>
                {
                    Success = false,
                    Message = "Failed to retrieve user statistics",
                    Data = null
                };

                return StatusCode(500, response);
            }
        }

        

        // New: Revenue statistics endpoint
        [HttpGet("revenue-statistics")]
        public async Task<ActionResult<ApiResponse<RevenueStatisticsDTO>>> GetRevenueStatistics()
        {
            try
            {
                var statistics = await _userService.GetRevenueStatisticsAsync();

                var response = new ApiResponse<RevenueStatisticsDTO>
                {
                    Success = true,
                    Message = "Revenue statistics retrieved successfully",
                    Data = statistics
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving revenue statistics");

                var response = new ApiResponse<RevenueStatisticsDTO>
                {
                    Success = false,
                    Message = "Failed to retrieve revenue statistics",
                    Data = null
                };

                return StatusCode(500, response);
            }
        }
    }
}
