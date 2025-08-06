using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class UserMealLog
{
    public int MealLogId { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly MealDate { get; set; }

    public string? MealType { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserMealItem> UserMealItems { get; set; } = new List<UserMealItem>();
}
