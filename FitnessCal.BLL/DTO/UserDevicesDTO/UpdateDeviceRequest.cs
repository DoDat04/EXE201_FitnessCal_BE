using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.UserDevicesDTO
{
    public class UpdateDeviceRequest
    {
        [Required]
        public Guid DeviceId { get; set; }

        [MaxLength(50)]
        public string? DeviceType { get; set; }

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        public bool? IsActive { get; set; }
    }
}
