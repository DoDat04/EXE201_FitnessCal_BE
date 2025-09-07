using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.SubscriptionDTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Lấy tất cả subscription của người dùng (Admin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUserSubscriptions()
        {
            try
            {
                var subscriptions = await _subscriptionService.GetAllUserSubscriptionsAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Lấy danh sách subscription thành công",
                    Data = subscriptions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Lỗi server khi lấy danh sách subscription",
                    Error = ex.Message
                });
            }
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserSubscriptionById(Guid userId)
        {
            try
            {
                var subscription = await _subscriptionService.GetUserSubscriptionByIdAsync(userId);
                return Ok(new
                {
                    Success = true,
                    Message = "Lấy thông tin subscription thành công",
                    Data = subscription
                });
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = knfEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Lỗi server khi lấy thông tin subscription",
                    Error = ex.Message
                });
            }
        }
    }
}
