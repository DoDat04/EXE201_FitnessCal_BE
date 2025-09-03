using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.AuthDTO.Request
{
    public class GoogleLoginRequestDTO
    {
        [Required]
        public string Uid { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
