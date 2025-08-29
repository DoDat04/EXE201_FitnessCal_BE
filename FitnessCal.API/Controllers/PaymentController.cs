using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.PaymentDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("buy-package")]
        public async Task<IActionResult> BuyPackage([FromQuery] int packageId)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized();
                }
                var res = await _paymentService.CreateSubscriptionAndInitPayment(userId, packageId);
                return Ok(new { success = true, data = res });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau." });
            }
        }

        [AllowAnonymous]
        [HttpPost("webhooks/payos")]
        public async Task<IActionResult> Webhook([FromBody] PayosWebhookPayload payload)
        {
            var ok = await _paymentService.HandlePayOSWebhook(payload);
            if (!ok) return BadRequest();
            return Ok(new { success = true });
        }

        [HttpGet("orders/{orderCode}/status")]
        public async Task<IActionResult> GetOrderStatus([FromRoute] int orderCode)
        {
            var res = await _paymentService.GetPaymentStatusByOrderCode(orderCode);
            return Ok(res);
        }

        [HttpGet("orders/{orderCode}/details")]
        public async Task<IActionResult> GetOrderDetails([FromRoute] int orderCode)
        {
            var res = await _paymentService.GetPaymentDetailsByOrderCode(orderCode);
            if (res == null) return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            return Ok(new { success = true, data = res });
        }

        [HttpPost("cancel-order/{orderCode}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderCode)
        {
            try
            {
                var result = await _paymentService.CancelOrder(orderCode);
                if (result)
                {
                    return Ok(new { success = true, message = "Đã hủy đơn hàng thành công" });
                }
                return BadRequest(new { success = false, message = "Không tìm thấy đơn hàng" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi hủy đơn hàng" });
            }
        }

        [HttpPost("confirm-order/{orderCode}")]
        public async Task<IActionResult> ConfirmOrder([FromRoute] int orderCode)
        {
            try
            {
                var result = await _paymentService.ConfirmOrder(orderCode);
                if (result)
                {
                    return Ok(new { success = true, message = "Đã xác nhận đơn hàng thành công" });
                }
                return BadRequest(new { success = false, message = "Không tìm thấy đơn hàng" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi xác nhận đơn hàng" });
            }
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments = await _paymentService.GetAllPayments();
                return Ok(new { success = true, data = payments });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách thanh toán" });
            }
        }
    }
}


