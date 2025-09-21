using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class Activity
{
    public int ActivityId { get; set; }

    public string Name { get; set; } = null!;

    public int DurationMinutes { get; set; }

    public int CaloriesBurned { get; set; }

    public virtual ICollection<UserActivity> UserActivities { get; set; } = new List<UserActivity>();
}
