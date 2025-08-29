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
        public string status { get; set; } = null!; 
        public long? paidAt { get; set; }
    }
}


