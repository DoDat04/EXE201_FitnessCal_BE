using System;
using System.Collections.Generic;

namespace FitnessCal.Domain
{
    public partial class Allergy
    {
        public int AllergyId { get; set; }
        public Guid UserId { get; set; }
        public int? FoodId { get; set; }
        public int? DishId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Food? Food { get; set; }
        public virtual PredefinedDish? Dish { get; set; }
    }
}
