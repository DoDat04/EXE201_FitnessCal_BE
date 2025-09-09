using FitnessCal.BLL.DTO.PaymentDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Define
{
    public interface IPaymentService
    {
        Task<InitPaymentResponse> CreateSubscriptionAndInitPayment(Guid userId, int packageId);

        Task<bool> HandlePayOSWebhook(PayosWebhookPayload payload);

        Task<PaymentStatusResponse> GetPaymentStatusByOrderCode(int orderCode);
        Task<PaymentDetailResponse?> GetPaymentDetailsByOrderCode(int orderCode);
        Task<List<GetAllPaymentsResponse>> GetAllPayments();
        Task CleanupExpiredPendingPaymentsAsync(int expirationMinutes = 30);
        Task<bool> CancelPaymentAsync(int orderCode, string? cancellationReason = null);
    }
}
