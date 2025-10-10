using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IUserDevicesRepository
    {
        Task<List<UserDevices>> GetByUserIdAsync(Guid userId);
        Task<List<UserDevices>> GetActiveByUserIdAsync(Guid userId);
        Task<UserDevices?> GetByUserIdAndTokenAsync(Guid userId, string fcmToken);
        Task<UserDevices> CreateAsync(UserDevices device);
        Task<UserDevices> UpdateAsync(UserDevices device);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid userId, string fcmToken);
        Task<List<UserDevices>> GetAllActiveDevicesAsync();
        Task<List<UserDevices>> GetActiveDevicesByUserIdsAsync(List<Guid> userIds);
        Task<bool> DeactivateDeviceAsync(Guid id);
        Task<bool> DeactivateAllUserDevicesAsync(Guid userId);
    }
}
