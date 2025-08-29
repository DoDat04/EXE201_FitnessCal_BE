using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.Domain
{
    public class PremiumPackage
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
