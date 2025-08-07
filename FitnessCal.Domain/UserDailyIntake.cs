using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class UserDailyIntake
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly MealDate { get; set; }

    public double? TotalCalories { get; set; }

    public double TargetCalories { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual User? User { get; set; }
}
