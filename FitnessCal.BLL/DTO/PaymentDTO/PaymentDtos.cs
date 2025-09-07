using System;

namespace FitnessCal.BLL.DTO.PaymentDTO
{
    public class InitPaymentResponse
    {
        public int OrderCode { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SubscriptionId { get; set; }
        public string CheckoutUrl { get; set; } = null!; 
    }

    public class PaymentStatusResponse
    {
        public int OrderCode { get; set; }
        public string? Status { get; set; }
    }

    public class PaymentDetailResponse
    {
        public int OrderCode { get; set; }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? PackageName { get; set; }
        public int? DurationMonths { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class GetAllPaymentsResponse
    {
        public int OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string UserEmail { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PackageName { get; set; } = null!;
        public int DurationMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PayosWebhookPayload
    {
        public int orderCode { get; set; }
        public string? status { get; set; } 
        public long? paidAt { get; set; }
        public string? code { get; set; }
        public string? desc { get; set; }
        public decimal? amount { get; set; }
        public string? description { get; set; }
        public string? accountNumber { get; set; }
        public string? reference { get; set; }
        public string? transactionDateTime { get; set; }
        public string? paymentLinkId { get; set; }
        public string? counterAccountBankId { get; set; }
        public string? counterAccountBankName { get; set; }
        public string? counterAccountName { get; set; }
        public string? counterAccountNumber { get; set; }
        public string? virtualAccountName { get; set; }
        public string? virtualAccountNumber { get; set; }
        public string? currency { get; set; }
        public string? signature { get; set; }
        public PayosWebhookData? data { get; set; }
    }

    public class PayosWebhookData
    {
        public int orderCode { get; set; }
        public decimal amount { get; set; }
        public string? description { get; set; }
        public string? accountNumber { get; set; }
        public string? reference { get; set; }
        public string? transactionDateTime { get; set; }
        public string? virtualAccountNumber { get; set; }
        public string? counterAccountBankId { get; set; }
        public string? counterAccountBankName { get; set; }
        public string? counterAccountName { get; set; }
        public string? counterAccountNumber { get; set; }
        public string? virtualAccountName { get; set; }
        public string? currency { get; set; }
        public string? paymentLinkId { get; set; }
        public string? code { get; set; }
        public string? desc { get; set; }
    }
}


