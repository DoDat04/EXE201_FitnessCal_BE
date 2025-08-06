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

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual UserHealth? UserHealth { get; set; }

    public virtual ICollection<UserMealLog> UserMealLogs { get; set; } = new List<UserMealLog>();
}
