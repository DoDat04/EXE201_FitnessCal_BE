using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FitnessCal.Domain;

public partial class UserMealItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ItemId { get; set; }

    public int? LogId { get; set; }

    public short IsCustom { get; set; }

    public int? DishId { get; set; }

    public int? FoodId { get; set; }

    public double Quantity { get; set; }

    public double? Calories { get; set; }

    public virtual PredefinedDish? Dish { get; set; }

    public virtual Food? Food { get; set; }

    public virtual UserMealLog? Log { get; set; }
}
