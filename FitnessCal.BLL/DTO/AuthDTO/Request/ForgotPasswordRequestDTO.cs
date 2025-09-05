using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.AuthDTO.Request
{
    public class ForgotPasswordRequestDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
}
