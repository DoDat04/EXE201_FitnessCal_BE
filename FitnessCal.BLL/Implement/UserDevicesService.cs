using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserDevicesDTO;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class UserDevicesService : IUserDevicesService
    {
        private readonly IUserDevicesRepository _userDevicesRepository;
        private readonly ILogger<UserDevicesService> _logger;

        public UserDevicesService(
            IUserDevicesRepository userDevicesRepository,
            ILogger<UserDevicesService> logger)
        {
            _userDevicesRepository = userDevicesRepository;
            _logger = logger;
        }

        public async Task<List<UserDevicesDTO>> GetUserDevicesAsync(Guid userId)
        {
            try
            {
                var devices = await _userDevicesRepository.GetByUserIdAsync(userId);
                return devices.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user devices for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<UserDevicesDTO>> GetActiveUserDevicesAsync(Guid userId)
        {
            try
            {
                var devices = await _userDevicesRepository.GetActiveByUserIdAsync(userId);
                return devices.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active user devices for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDevicesDTO> RegisterDeviceAsync(Guid userId, RegisterDeviceRequest request)
        {
            try
            {
                // Kiểm tra xem device đã tồn tại chưa
                var existingDevice = await _userDevicesRepository.GetByUserIdAndTokenAsync(userId, request.FcmToken);
                if (existingDevice != null)
                {
                    // Reactivate device nếu đã tồn tại nhưng inactive
                    if (!existingDevice.IsActive)
                    {
                        existingDevice.IsActive = true;
                        existingDevice.DeviceType = request.DeviceType;
                        existingDevice.DeviceName = request.DeviceName;
                        existingDevice = await _userDevicesRepository.UpdateAsync(existingDevice);
                    }
                    return MapToDTO(existingDevice);
                }

                // Tạo device mới
                var device = new UserDevices
                {
                    UserId = userId,
                    FcmToken = request.FcmToken,
                    DeviceType = request.DeviceType,
                    DeviceName = request.DeviceName,
                    IsActive = true
                };

                var createdDevice = await _userDevicesRepository.CreateAsync(device);
                return MapToDTO(createdDevice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDevicesDTO> UpdateDeviceAsync(Guid userId, UpdateDeviceRequest request)
        {
            try
            {
                var device = await _userDevicesRepository.GetByUserIdAsync(userId)
                    .ContinueWith(t => t.Result.FirstOrDefault(d => d.Id == request.DeviceId));

                if (device == null)
                    throw new ArgumentException("Device not found");

                // Cập nhật các field được cung cấp
                if (request.DeviceType != null)
                    device.DeviceType = request.DeviceType;
                
                if (request.DeviceName != null)
                    device.DeviceName = request.DeviceName;
                
                if (request.IsActive.HasValue)
                    device.IsActive = request.IsActive.Value;

                var updatedDevice = await _userDevicesRepository.UpdateAsync(device);
                return MapToDTO(updatedDevice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device {DeviceId} for user {UserId}", request.DeviceId, userId);
                throw;
            }
        }

        public async Task<bool> DeleteDeviceAsync(Guid userId, Guid deviceId)
        {
            try
            {
                var device = await _userDevicesRepository.GetByUserIdAsync(userId)
                    .ContinueWith(t => t.Result.FirstOrDefault(d => d.Id == deviceId));

                if (device == null)
                    return false;

                return await _userDevicesRepository.DeleteAsync(deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting device {DeviceId} for user {UserId}", deviceId, userId);
                throw;
            }
        }

        public async Task<bool> DeactivateDeviceAsync(Guid userId, Guid deviceId)
        {
            try
            {
                var device = await _userDevicesRepository.GetByUserIdAsync(userId)
                    .ContinueWith(t => t.Result.FirstOrDefault(d => d.Id == deviceId));

                if (device == null)
                    return false;

                return await _userDevicesRepository.DeactivateDeviceAsync(deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating device {DeviceId} for user {UserId}", deviceId, userId);
                throw;
            }
        }

        public async Task<bool> DeactivateAllUserDevicesAsync(Guid userId)
        {
            try
            {
                return await _userDevicesRepository.DeactivateAllUserDevicesAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating all devices for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<string>> GetActiveFcmTokensAsync(Guid userId)
        {
            try
            {
                var devices = await _userDevicesRepository.GetActiveByUserIdAsync(userId);
                return devices.Select(d => d.FcmToken).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active FCM tokens for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<string>> GetActiveFcmTokensByUserIdsAsync(List<Guid> userIds)
        {
            try
            {
                var devices = await _userDevicesRepository.GetActiveDevicesByUserIdsAsync(userIds);
                return devices.Select(d => d.FcmToken).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active FCM tokens for users {UserIds}", string.Join(", ", userIds));
                throw;
            }
        }

        public async Task<bool> IsDeviceRegisteredAsync(Guid userId, string fcmToken)
        {
            try
            {
                return await _userDevicesRepository.ExistsAsync(userId, fcmToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking device registration for user {UserId}", userId);
                return false;
            }
        }

        private static UserDevicesDTO MapToDTO(UserDevices device)
        {
            return new UserDevicesDTO
            {
                Id = device.Id,
                UserId = device.UserId,
                FcmToken = device.FcmToken,
                DeviceType = device.DeviceType,
                DeviceName = device.DeviceName,
                IsActive = device.IsActive,
                CreatedAt = device.CreatedAt
            };
        }
    }
}
