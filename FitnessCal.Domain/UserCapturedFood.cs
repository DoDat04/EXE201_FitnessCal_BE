using System;

namespace FitnessCal.Domain;

public partial class UserCapturedFood
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public double Calories { get; set; }

    public double Carbs { get; set; }

    public double Fat { get; set; }

    public double Protein { get; set; }

    public virtual User User { get; set; } = null!;
}
