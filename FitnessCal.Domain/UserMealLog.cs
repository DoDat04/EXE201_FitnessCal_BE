using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FitnessCal.Domain;

public partial class UserMealLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LogId { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly MealDate { get; set; }

    public string? MealType { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserMealItem> UserMealItems { get; set; } = new List<UserMealItem>();
}
