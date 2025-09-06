using FitnessCal.Domain;

namespace FitnessCal.BLL.DTO.SubscriptionDTO.Response
{
    public class UserSubscriptionResponseDTO
    {
        public int SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public PackageInfoDTO Package { get; set; } = null!;
        public decimal PriceAtPurchase { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public bool IsActive { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsUserBanned { get; set; }
    }

    public class PackageInfoDTO
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Price { get; set; }
        public string PackageType { get; set; } = null!; // "Free", "Premium"
    }
}
