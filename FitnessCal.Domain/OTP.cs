using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCal.Domain
{
    public class OTP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6)]
        public string OTPCode { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } = null!; 

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UsedAt { get; set; }
    }
}
