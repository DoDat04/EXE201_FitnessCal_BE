using System;
using System.Collections.Generic;

namespace FitnessCal.Domain;

public partial class User
{
    public Guid UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public short IsActive { get; set; }  

    public DateTime CreatedAt { get; set; }

    public string? SupabaseUserId { get; set; }

    public virtual ICollection<UserDailyIntake> UserDailyIntakes { get; set; } = new List<UserDailyIntake>();

    public virtual UserHealth? UserHealth { get; set; }

    public virtual ICollection<UserMealLog> UserMealLogs { get; set; } = new List<UserMealLog>();

    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();

    public virtual ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
}
