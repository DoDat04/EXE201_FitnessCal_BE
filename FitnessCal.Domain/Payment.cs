using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.Domain
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public int PayosOrderCode { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public virtual UserSubscription Subscription { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
