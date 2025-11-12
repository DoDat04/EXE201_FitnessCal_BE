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
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IUserService userService, ILogger<DashboardController> logger, ISubscriptionService subscriptionService)
        {
            _userService = userService;
            _logger = logger;
            _subscriptionService = subscriptionService;
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
        public async Task<ActionResult<ApiResponse<RevenueStatisticsDTO>>> GetRevenueStatistics(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _userService.GetRevenueStatisticsAsync(startDate, endDate);

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

        // New: Conversion rate endpoint for admin dashboard
        [HttpGet("conversion-rate")]
        public async Task<ActionResult<ApiResponse<ConversionRateResponseDTO>>> GetConversionRate()
        {
            try
            {
                var conversionData = await _userService.GetConversionRateAsync();

                var response = new ApiResponse<ConversionRateResponseDTO>
                {
                    Success = true,
                    Message = "Conversion rate data retrieved successfully",
                    Data = conversionData
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving conversion rate data");

                var response = new ApiResponse<ConversionRateResponseDTO>
                {
                    Success = false,
                    Message = "Failed to retrieve conversion rate data",
                    Data = null
                };

                return StatusCode(500, response);
            }
        }
        [HttpGet("total-subscriptions-payments")]
        public async Task<ActionResult<ApiResponse<int>>> GetTotalSubscriptionsPayments()
        {
            try
            {
                var totalPayments = await _subscriptionService.GetTotalSubscriptionsPaymentsAsync();
                var response = new ApiResponse<int>
                {
                    Success = true,
                    Message = "Total subscriptions payments retrieved successfully",
                    Data = totalPayments
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving total subscriptions payments");
                var response = new ApiResponse<int>
                {
                    Success = false,
                    Message = "Failed to retrieve total subscriptions payments",
                    Data = 0
                };
                return StatusCode(500, response);
            }
        }
    }
}
