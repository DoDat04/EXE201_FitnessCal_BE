using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.Extensions.Logging;
using System;

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
        public async Task<ActionResult<DashboardResponseDTO>> GetUserStatistics()
        {
            try
            {
                var statistics = await _userService.GetUserStatisticsAsync();
                
                var response = new DashboardResponseDTO
                {
                    UserStatistics = statistics,
                    IsSuccess = true,
                    Message = "User statistics retrieved successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user statistics");
                
                var response = new DashboardResponseDTO
                {
                    IsSuccess = false,
                    Message = "Failed to retrieve user statistics"
                };

                return StatusCode(500, response);
            }
        }
    }
}
