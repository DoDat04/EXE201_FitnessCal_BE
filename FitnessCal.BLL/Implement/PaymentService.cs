using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.PaymentDTO;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace FitnessCal.BLL.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPayosService _payosService;
        private readonly PayOSSettings _payosSettings;
        private readonly IEmailService _emailService;

        public PaymentService(IUnitOfWork uow, IPayosService payosService, IOptions<PayOSSettings> payosSettings, IEmailService emailService)
        {
            _uow = uow;
            _payosService = payosService;
            _payosSettings = payosSettings.Value;
            _emailService = emailService;
        }

        public async Task<InitPaymentResponse> CreateSubscriptionAndInitPayment(Guid userId, int packageId)
        {
            await _uow.Payments.CleanupExpiredPendingPaymentsAsync();

            // Lấy thông tin gói trước để kiểm tra
            var pkg = await _uow.PremiumPackages.GetByIdAsync(packageId);
            if (pkg == null)
                throw new InvalidOperationException("Không tìm thấy gói premium. Vui lòng kiểm tra lại thông tin.");
            
            // gói Free là trạng thái mặc định)
            if (pkg.Price == 0)
                throw new InvalidOperationException("Gói miễn phí không thể mua được. Đây là trạng thái mặc định của tài khoản.");

            var active = await _uow.UserSubscriptions.GetActivePaidByUserAsync(userId);
            if (active != null)
            {
                // Kiểm tra xem gói hiện tại có phải gói Free không (cho người dùng cũ)
                var currentPackage = await _uow.PremiumPackages.GetByIdAsync(active.PackageId);
                if (currentPackage != null && currentPackage.Price == 0)
                {
                    // Nếu đang có gói Free cũ và muốn mua gói trả phí, cho phép
                    // Cập nhật gói Free thành failed để có thể mua gói mới
                    active.PaymentStatus = "failed";
                    await _uow.UserSubscriptions.UpdateAsync(active);
                    await _uow.Save();
                }
                else
                {
                    throw new InvalidOperationException("Bạn đang có gói premium còn hạn. Vui lòng đợi gói hiện tại hết hạn trước khi mua gói mới.");
                }
            }

            // Kiểm tra xem có subscription nào với status failed hoặc expired không
            var inactiveSubscriptions = await _uow.UserSubscriptions.GetAllAsync(s => 
                s.UserId == userId && 
                (s.PaymentStatus == "failed" || s.PaymentStatus == "expired"));
            
            if (inactiveSubscriptions.Any())
            {
                // Nếu có subscription failed/expired, cho phép mua gói mới
                // Không cần làm gì thêm, chỉ cần cho phép tiếp tục
            }

            if (await _uow.UserSubscriptions.HasPendingByUserAsync(userId))
                throw new InvalidOperationException("Bạn đang có đơn hàng đang xử lý. Vui lòng hoàn tất thanh toán hoặc đợi đơn hàng được xử lý.");

            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.");

            var start = DateTime.UtcNow.Date;
            var end = start.AddMonths(pkg.DurationMonths);

            var sub = new UserSubscription
            {
                UserId = userId,
                PackageId = pkg.PackageId,
                PriceAtPurchase = pkg.Price,
                StartDate = start,
                EndDate = end,
                PaymentStatus = pkg.Price == 0 ? "paid" : "pending" // Gói miễn phí tự động thành paid
            };

            await _uow.UserSubscriptions.AddAsync(sub);
            await _uow.Save();

            // Xử lý gói trả phí
            var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            
            var durationText = GetDurationText(pkg.DurationMonths);

            var payosRequest = new CreatePayOSPaymentRequest
            {
                OrderCode = orderCode,
                Amount = pkg.Price,
                Description = $"Gói Premium {durationText}",
                ReturnUrl = _payosSettings.ReturnUrl.Replace("{orderCode}", orderCode.ToString()), 
                CancelUrl = _payosSettings.CancelUrl.Replace("{orderCode}", orderCode.ToString()),
                Items = new List<PayOSItem>
                {
                    new PayOSItem($"Gói Premium {durationText}", 1, pkg.Price)
                }
            };

            var payosResponse = await _payosService.CreatePaymentLinkAsync(payosRequest);

            var paymentPaid = new Payment
            {
                SubscriptionId = sub.SubscriptionId,
                UserId = userId,
                Amount = pkg.Price,
                PayosOrderCode = payosResponse.OrderCode,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Payments.AddAsync(paymentPaid);
            await _uow.Save();

            return new InitPaymentResponse
            {
                OrderCode = payosResponse.OrderCode,
                Amount = pkg.Price,
                StartDate = start,
                EndDate = end,
                SubscriptionId = sub.SubscriptionId,
                CheckoutUrl = payosResponse.CheckoutUrl
            };
        }

        public async Task<bool> HandlePayOSWebhook(PayosWebhookPayload payload)
        {
            // PayOS gửi orderCode trong data.orderCode, không phải payload.orderCode
            var actualOrderCode = payload.data?.orderCode ?? payload.orderCode;
            
            // Log webhook payload for debugging
            Console.WriteLine($"PayOS Webhook received - OrderCode: {actualOrderCode}, Status: {payload.status}, Code: {payload.code}, Desc: {payload.desc}");
            Console.WriteLine($"Data orderCode: {payload.data?.orderCode}, Success: {payload.success}");
            
            if (actualOrderCode == 0)
            {
                Console.WriteLine("OrderCode is 0, skipping processing");
                return true;
            }
            
            var payment = await _uow.Payments.GetByOrderCodeAsync(actualOrderCode);
            if (payment == null) 
            {
                return true;
            }

            string newStatus;
            
            // PayOS dùng success field và code/desc để xác định trạng thái
            if (payload.success == true && payload.code == "00" && payload.desc?.ToLowerInvariant().Contains("success") == true)
            {
                newStatus = "paid";
            }
            else if (payload.success == false || payload.code != "00" || payload.desc?.ToLowerInvariant().Contains("success") != true)
            {
                newStatus = "failed";
            }
            else
            {
                Console.WriteLine($"Cannot determine status from payload: success={payload.success}, code={payload.code}, desc={payload.desc}");
                return false;
            }

            Console.WriteLine($"Processing status change: {payment.Status} -> {newStatus}");
            
            if (payment.Status == newStatus) 
            {
                Console.WriteLine($"Status unchanged, skipping processing");
                return true;
            }

            if (newStatus == "paid")
            {
                var paidAt = payload.paidAt.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(payload.paidAt.Value)
                    : DateTimeOffset.UtcNow;
                    
                await _uow.Payments.MarkPaidAsync(payment.PaymentId, paidAt);

                var sub = await _uow.UserSubscriptions.GetByIdAsync(payment.SubscriptionId);
                if (sub != null)
                {
                    sub.PaymentStatus = "paid";
                    await _uow.UserSubscriptions.UpdateAsync(sub);
                    await _uow.Save();

                    // Gửi email xác nhận thanh toán thành công
                    var user = await _uow.Users.GetByIdAsync(payment.UserId);
                    var pkg = await _uow.PremiumPackages.GetByIdAsync(sub.PackageId);
                    if (user != null && pkg != null && !string.IsNullOrWhiteSpace(user.Email))
                    {
                        var subject = "Xác nhận thanh toán - FitnessCal";
                        var html = BuildPaymentSuccessEmailHtml(
                            user: user,
                            orderCode: payment.PayosOrderCode,
                            amount: payment.Amount,
                            packageName: pkg.Name,
                            durationMonths: pkg.DurationMonths,
                            createdAt: payment.CreatedAt,
                            paidAt: paidAt.UtcDateTime,
                            startDate: sub.StartDate,
                            endDate: sub.EndDate
                        );

                        _ = _emailService.SendEmailAsync(user.Email, subject, html);
                    }
                }
                
                return true;
            }
            else if (newStatus == "failed" || newStatus == "refunded" || newStatus == "cancelled" || newStatus == "expired")
            {
                await _uow.Payments.MarkFailedAsync(payment.PaymentId);
                var sub = await _uow.UserSubscriptions.GetByIdAsync(payment.SubscriptionId);
                if (sub != null)
                {
                    sub.PaymentStatus = "failed";
                    await _uow.UserSubscriptions.UpdateAsync(sub);
                    await _uow.Save();
                }
                return true;
            }

            return false;
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusByOrderCode(int orderCode)
        {
            var status = await _uow.Payments.GetStatusByOrderCodeAsync(orderCode);
            return new PaymentStatusResponse
            {
                OrderCode = orderCode,
                Status = status
            };
        }

        public async Task<PaymentDetailResponse?> GetPaymentDetailsByOrderCode(int orderCode)
        {
            var payment = await _uow.Payments.GetByOrderCodeAsync(orderCode);
            if (payment == null) return null;

            var sub = await _uow.UserSubscriptions.GetByIdAsync(payment.SubscriptionId);
            PremiumPackage? pkg = null;
            if (sub != null)
            {
                pkg = await _uow.PremiumPackages.GetByIdAsync(sub.PackageId);
            }

            return new PaymentDetailResponse
            {
                OrderCode = payment.PayosOrderCode,
                Status = payment.Status,
                Amount = payment.Amount,
                PackageName = pkg?.Name,
                DurationMonths = pkg?.DurationMonths,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt
            };
        }

        public async Task<List<GetAllPaymentsResponse>> GetAllPayments()
        {
            var payments = await _uow.Payments.GetAllWithDetailsAsync();
            var result = new List<GetAllPaymentsResponse>();

            foreach (var payment in payments)
            {
                var sub = await _uow.UserSubscriptions.GetByIdAsync(payment.SubscriptionId);
                var user = await _uow.Users.GetByIdAsync(payment.UserId);
                PremiumPackage? pkg = null;
                
                if (sub != null)
                {
                    pkg = await _uow.PremiumPackages.GetByIdAsync(sub.PackageId);
                }

                result.Add(new GetAllPaymentsResponse
                {
                    OrderCode = payment.PayosOrderCode,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    CreatedAt = payment.CreatedAt,
                    PaidAt = payment.PaidAt,
                    UserEmail = user?.Email ?? "N/A",
                    UserName = $"{user?.FirstName ?? ""} {user?.LastName ?? ""}".Trim() ?? "N/A",
                    PackageName = pkg?.Name ?? "N/A",
                    DurationMonths = pkg?.DurationMonths ?? 0,
                    StartDate = sub?.StartDate ?? DateTime.MinValue,
                    EndDate = sub?.EndDate ?? DateTime.MinValue
                });
            }

            return result;
        }

        public async Task CleanupExpiredPendingPaymentsAsync(int expirationMinutes = 30)
        {
            await _uow.Payments.CleanupExpiredPendingPaymentsAsync(expirationMinutes);
        }

        public async Task<bool> CancelPaymentAsync(int orderCode, string? cancellationReason = null)
        {
            try
            {
                var payment = await _uow.Payments.GetByOrderCodeAsync(orderCode);
                if (payment == null)
                {
                    Console.WriteLine($"Payment not found for orderCode: {orderCode}");
                    return false;
                }

                if (payment.Status != "pending")
                {
                    Console.WriteLine($"Payment {orderCode} is already {payment.Status}, cannot cancel");
                    return false;
                }

                // Hủy link thanh toán trên PayOS
                var cancelSuccess = await _payosService.CancelPaymentLinkAsync(orderCode, cancellationReason);
                if (!cancelSuccess)
                {
                    Console.WriteLine($"Failed to cancel PayOS payment link for orderCode: {orderCode}");
                    return false;
                }

                // Cập nhật trạng thái trong database
                await _uow.Payments.MarkFailedAsync(payment.PaymentId);
                
                var sub = await _uow.UserSubscriptions.GetByIdAsync(payment.SubscriptionId);
                if (sub != null)
                {
                    sub.PaymentStatus = "failed";
                    await _uow.UserSubscriptions.UpdateAsync(sub);
                    await _uow.Save();
                }

                Console.WriteLine($"Successfully cancelled payment for orderCode: {orderCode}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling payment {orderCode}: {ex.Message}");
                return false;
            }
        }

        private string BuildPaymentSuccessEmailHtml(
            User user,
            int orderCode,
            decimal amount,
            string packageName,
            int durationMonths,
            DateTime createdAt,
            DateTime paidAt,
            DateTime startDate,
            DateTime endDate)
        {
            // Tìm template trong thư mục project, không phải bin/Debug
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
            var templatePath = Path.Combine(projectRoot, "FitnessCal.BLL", "Templates", "PaymentSuccessEmailTemplate.html");
            
            // Fallback: tìm trong thư mục hiện tại
            if (!File.Exists(templatePath))
            {
                templatePath = Path.Combine(baseDirectory, "Templates", "PaymentSuccessEmailTemplate.html");
            }
            
            // Fallback cuối: tìm trong thư mục BLL
            if (!File.Exists(templatePath))
            {
                var bllPath = Path.Combine(projectRoot, "FitnessCal.BLL", "Templates", "PaymentSuccessEmailTemplate.html");
                if (File.Exists(bllPath))
                {
                    templatePath = bllPath;
                }
            }
            
            Console.WriteLine($"Looking for template at: {templatePath}");
            Console.WriteLine($"Template exists: {File.Exists(templatePath)}");
            
            var html = File.ReadAllText(templatePath);

            var vi = new CultureInfo("vi-VN");
            var amountVnd = string.Format(vi, "{0:C0}", amount);
            var durationText = $"({GetDurationText(durationMonths)})";

            // Chuyển thời gian UTC sang giờ Việt Nam (UTC+7)
            var createdAtVN = ConvertUtcToVietnamTime(createdAt);
            var paidAtVN = ConvertUtcToVietnamTime(paidAt);
            var startDateVN = ConvertUtcToVietnamTime(startDate);
            var endDateVN = ConvertUtcToVietnamTime(endDate);

            html = html
                .Replace("{UserName}", $"{(user.FirstName ?? string.Empty)} {(user.LastName ?? string.Empty)}".Trim())
                .Replace("{PackageName}", packageName ?? string.Empty)
                .Replace("{DurationText}", durationText)
                .Replace("{Amount}", amountVnd)
                .Replace("{OrderCode}", orderCode.ToString())
                .Replace("{CreatedAt}", createdAtVN.ToString("HH:mm dd/MM/yyyy"))
                .Replace("{PaidAt}", paidAtVN.ToString("HH:mm dd/MM/yyyy"))
                .Replace("{StartDate}", startDateVN.ToString("dd/MM/yyyy"))
                .Replace("{EndDate}", endDateVN.ToString("dd/MM/yyyy"))
                .Replace("{Year}", DateTime.Now.Year.ToString());

            return html;
        }

        private static DateTime ConvertUtcToVietnamTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                    return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
                }
                catch
                {
                    return utcDateTime.AddHours(7);
                }
            }
        }

        private string GetDurationText(int durationMonths)
        {
            if (durationMonths == 0)
                return "miễn phí";
            else if (durationMonths == 0.25)
                return "1 tuần";
            else if (durationMonths == 1)
                return "1 tháng";
            else
                return $"{durationMonths} tháng";
        }


    }
}
