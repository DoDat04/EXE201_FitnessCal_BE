using System.ComponentModel.DataAnnotations;

namespace FitnessCal.Domain
{
    public class UserDevices
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FcmToken { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DeviceType { get; set; } // 'ios', 'android', 'web'

        [MaxLength(100)]
        public string? DeviceName { get; set; } // 'iPhone 15', 'Samsung Galaxy', 'Chrome Browser'

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User? User { get; set; }
    }
}
