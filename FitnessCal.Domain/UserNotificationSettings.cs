using System.ComponentModel.DataAnnotations;

namespace FitnessCal.Domain
{
    public class UserNotificationSettings
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public bool IsNotificationEnabled { get; set; } = false;

        public bool BreakfastNotification { get; set; } = false;

        public bool LunchNotification { get; set; } = false;

        public bool DinnerNotification { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User? User { get; set; }
    }
}
