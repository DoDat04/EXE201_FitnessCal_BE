using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.Domain
{
    public class UserSubscription
    {
        public int SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public int PackageId { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PaymentStatus { get; set; } = "pending";

        public virtual User User { get; set; } = null!;
        public virtual PremiumPackage Package { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
