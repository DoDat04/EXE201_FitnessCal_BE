using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.UserDevicesDTO
{
    public class RegisterDeviceRequest
    {
        [Required]
        [MaxLength(255)]
        public string FcmToken { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DeviceType { get; set; } // 'ios', 'android', 'web'

        [MaxLength(100)]
        public string? DeviceName { get; set; }
    }
}
