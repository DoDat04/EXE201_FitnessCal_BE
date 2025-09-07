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
        [HttpGet("webhooks/payos")]
        [HttpPost("webhooks/payos")]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                var method = Request.Method;
                Console.WriteLine($"Webhook {method} received");
                
                if (Request.Headers.ContainsKey("ngrok-skip-browser-warning"))
                {
                    Response.Headers["ngrok-skip-browser-warning"] = "true";
                }
                
                if (method == "POST")
                {
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    
                    if (!string.IsNullOrEmpty(body))
                    {
                        try
                        {
                            var payload = System.Text.Json.JsonSerializer.Deserialize<PayosWebhookPayload>(body);
                            if (payload != null)
                            {
                                var ok = await _paymentService.HandlePayOSWebhook(payload);
                                if (!ok) return BadRequest(new { success = false, message = "Failed to process webhook" });
                            }
                            else
                            {
                                return BadRequest(new { success = false, message = "Invalid payload format" });
                            }
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
                        }
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "Empty body" });
                    }
                }
                
                return Ok(new { 
                    success = true, 
                    message = $"Webhook processed successfully - {method}",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Webhook {Request.Method} error: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
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

        [HttpPost("cleanup-expired")]
        public async Task<IActionResult> CleanupExpiredPendingPayments([FromQuery] int expirationMinutes = 30)
        {
            try
            {
                await _paymentService.CleanupExpiredPendingPaymentsAsync(expirationMinutes);
                return Ok(new { success = true, message = $"Đã cleanup các đơn pending đã hết hạn (hơn {expirationMinutes} phút)" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Đã xảy ra lỗi khi cleanup: {ex.Message}" });
            }
        }
    }
}


