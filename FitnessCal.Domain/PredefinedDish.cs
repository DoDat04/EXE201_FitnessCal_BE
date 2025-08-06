using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class PredefinedDish
{
    public int DishId { get; set; }

    public string Name { get; set; } = null!;

    public double Calories { get; set; }

    public double Carbs { get; set; }

    public double Fat { get; set; }

    public double Protein { get; set; }

    public string? ServingUnit { get; set; }

    public virtual ICollection<UserMealItem> UserMealItems { get; set; } = new List<UserMealItem>();
}
