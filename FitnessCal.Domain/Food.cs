using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class Food
{
    public int FoodId { get; set; }

    public string Name { get; set; } = null!;

    public double Calories { get; set; }

    public double Carbs { get; set; }

    public double Fat { get; set; }

    public double Protein { get; set; }

    public string? FoodCategory { get; set; }

    public virtual ICollection<UserMealItem> UserMealItems { get; set; } = new List<UserMealItem>();
}
