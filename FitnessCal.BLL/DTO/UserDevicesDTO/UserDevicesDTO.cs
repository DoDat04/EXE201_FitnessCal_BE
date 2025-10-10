namespace FitnessCal.BLL.DTO.UserDevicesDTO
{
    public class UserDevicesDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FcmToken { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
