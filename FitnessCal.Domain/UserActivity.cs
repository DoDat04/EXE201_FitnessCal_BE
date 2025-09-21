using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class UserActivity
{
    public int UserActivityId { get; set; }

    public Guid UserId { get; set; }

    public int ActivityId { get; set; }

    public DateOnly ActivityDate { get; set; }

    public int DurationMinutes { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
