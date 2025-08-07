using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class UserMealItem
{
    public int ItemId { get; set; }

    public int? LogId { get; set; }

    public bool IsCustom { get; set; }

    public int? DishId { get; set; }

    public int? FoodId { get; set; }

    public double Quantity { get; set; }

    public double? Calories { get; set; }

    public virtual PredefinedDish? Dish { get; set; }

    public virtual Food? Food { get; set; }

    public virtual UserMealLog? Log { get; set; }
}
