using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.PaymentDTO;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitnessCal.BLL.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPayosService _payosService;
        private readonly PayOSSettings _payosSettings;

        public PaymentService(IUnitOfWork uow, IPayosService payosService, IOptions<PayOSSettings> payosSettings)
        {
            _uow = uow;
            _payosService = payosService;
            _payosSettings = payosSettings.Value;
        }

        public async Task<InitPaymentResponse> CreateSubscriptionAndInitPayment(Guid userId, int packageId)
        {
            var active = await _uow.UserSubscriptions.GetActivePaidByUserAsync(userId);
            if (active != null)
                throw new InvalidOperationException("Bạn đang có gói premium còn hạn. Vui lòng đợi gói hiện tại hết hạn trước khi mua gói mới.");

            if (await _uow.UserSubscriptions.HasPendingByUserAsync(userId))
                throw new InvalidOperationException("Bạn đang có đơn hàng đang xử lý. Vui lòng hoàn tất thanh toán hoặc đợi đơn hàng được xử lý.");

            var pkg = await _uow.PremiumPackages.GetByIdAsync(packageId);
            if (pkg == null)
                throw new InvalidOperationException("Không tìm thấy gói premium. Vui lòng kiểm tra lại thông tin.");

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
                PaymentStatus = "pending"
            };

            await _uow.UserSubscriptions.AddAsync(sub);
            await _uow.Save();

            var payosRequest = new CreatePayOSPaymentRequest
            {
                Amount = pkg.Price,
                Description = $"Gói {pkg.Name} - {pkg.DurationMonths} tháng",
                ReturnUrl = _payosSettings.ReturnUrl.Replace("{orderCode}", "{orderCode}"), 
                CancelUrl = _payosSettings.CancelUrl,
                Items = new List<PayOSItem>
                {
                    new PayOSItem($"Gói {pkg.Name}", 1, pkg.Price)
                }
            };

            var payosResponse = await _payosService.CreatePaymentLinkAsync(payosRequest);

            var payment = new Payment
            {
                SubscriptionId = sub.SubscriptionId,
                UserId = userId,
                Amount = pkg.Price,
                PayosOrderCode = payosResponse.OrderCode,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Payments.AddAsync(payment);
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
            var payment = await _uow.Payments.GetByOrderCodeAsync(payload.orderCode);
            if (payment == null) return false;

            var newStatus = payload.status?.ToLowerInvariant();
            if (string.IsNullOrEmpty(newStatus)) return false;

            if (payment.Status == newStatus) return true;

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
                }
                return true;
            }
            else if (newStatus == "failed" || newStatus == "refunded")
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

        public async Task<bool> CancelOrder(int orderCode)
        {
            var payment = await _uow.Payments.GetByOrderCodeAsync(orderCode);
            if (payment == null) return false;

            payment.Status = "failed";
            await _uow.Payments.UpdateAsync(payment);

            var sub = await _uow.UserSubscriptions.GetByIdAsync(payment.SubscriptionId);
            if (sub != null)
            {
                sub.PaymentStatus = "failed";
                await _uow.UserSubscriptions.UpdateAsync(sub);
            }

            await _uow.Save();
            return true;
        }

        public async Task<bool> ConfirmOrder(int orderCode)
        {
            var payment = await _uow.Payments.GetByOrderCodeAsync(orderCode);
            if (payment == null) return false;

            payment.Status = "paid";
            payment.PaidAt = DateTime.UtcNow;
            await _uow.Payments.UpdateAsync(payment);

            var sub = await _uow.UserSubscriptions.GetByIdAsync(payment.SubscriptionId);
            if (sub != null)
            {
                sub.PaymentStatus = "paid";
                await _uow.UserSubscriptions.UpdateAsync(sub);
            }

            await _uow.Save();
            return true;
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
    }
}
