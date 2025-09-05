using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.AuthDTO.Request
{
    public class VerifyOTPRequestDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 ký tự")]
        public string OTPCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mục đích sử dụng là bắt buộc")]
        public string Purpose { get; set; } = string.Empty; // "REGISTER", "RESET_PASSWORD", "CHANGE_EMAIL"
    }
}
