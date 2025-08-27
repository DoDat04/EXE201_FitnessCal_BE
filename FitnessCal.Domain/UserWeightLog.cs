using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.Domain
{
    public partial class UserWeightLog
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }

        public decimal WeightKg { get; set; }

        public DateOnly LogDate { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
