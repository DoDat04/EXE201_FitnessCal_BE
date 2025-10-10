using FitnessCal.BLL.DTO.UserDevicesDTO;

namespace FitnessCal.BLL.Define
{
    public interface IUserDevicesService
    {
        Task<List<UserDevicesDTO>> GetUserDevicesAsync(Guid userId);
        Task<List<UserDevicesDTO>> GetActiveUserDevicesAsync(Guid userId);
        Task<UserDevicesDTO> RegisterDeviceAsync(Guid userId, RegisterDeviceRequest request);
        Task<UserDevicesDTO> UpdateDeviceAsync(Guid userId, UpdateDeviceRequest request);
        Task<bool> DeleteDeviceAsync(Guid userId, Guid deviceId);
        Task<bool> DeactivateDeviceAsync(Guid userId, Guid deviceId);
        Task<bool> DeactivateAllUserDevicesAsync(Guid userId);
        Task<List<string>> GetActiveFcmTokensAsync(Guid userId);
        Task<List<string>> GetActiveFcmTokensByUserIdsAsync(List<Guid> userIds);
        Task<bool> IsDeviceRegisteredAsync(Guid userId, string fcmToken);
    }
}
