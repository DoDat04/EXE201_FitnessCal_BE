using System;
using System.Collections.Generic;

namespace FitnessCal.Domain
{
    public partial class FavoriteFood
    {
        public int FavoriteId { get; set; }
        public Guid UserId { get; set; }
        public int FoodId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Food Food { get; set; } = null!;
    }
}
