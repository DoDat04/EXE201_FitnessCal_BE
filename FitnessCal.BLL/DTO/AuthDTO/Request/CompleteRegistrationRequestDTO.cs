using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.AuthDTO.Request
{
    public class CompleteRegistrationRequestDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration token là bắt buộc")]
        public string RegistrationToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 số")]
        public string OTPCode { get; set; } = string.Empty;
    }
}
