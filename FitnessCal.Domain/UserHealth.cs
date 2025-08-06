using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class UserHealth
{
    public Guid UserId { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public decimal? HeightCm { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? TargetWeightKg { get; set; }

    public string? ActivityLevel { get; set; }

    public string? Goal { get; set; }

    public string? DietType { get; set; }

    public string? IntensityLevel { get; set; }

    public DateOnly? EstimatedGoalDate { get; set; }

    public string? GoalNote { get; set; }

    public virtual User User { get; set; } = null!;
}
