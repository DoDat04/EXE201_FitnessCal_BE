using Microsoft.EntityFrameworkCore;
using FitnessCal.Domain;
using FitnessCal.DAL.Define;

namespace FitnessCal.DAL.Implement
{
    public class UserDevicesRepository : IUserDevicesRepository
    {
        private readonly FitnessCalContext _context;

        public UserDevicesRepository(FitnessCalContext context)
        {
            _context = context;
        }

        public async Task<List<UserDevices>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserDevices
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserDevices>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserDevices?> GetByUserIdAndTokenAsync(Guid userId, string fcmToken)
        {
            return await _context.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.FcmToken == fcmToken);
        }

        public async Task<UserDevices> CreateAsync(UserDevices device)
        {
            device.CreatedAt = DateTime.UtcNow;
            _context.UserDevices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<UserDevices> UpdateAsync(UserDevices device)
        {
            _context.UserDevices.Update(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var device = await _context.UserDevices.FindAsync(id);
            if (device == null)
                return false;

            _context.UserDevices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid userId, string fcmToken)
        {
            return await _context.UserDevices
                .AnyAsync(d => d.UserId == userId && d.FcmToken == fcmToken);
        }

        public async Task<List<UserDevices>> GetAllActiveDevicesAsync()
        {
            return await _context.UserDevices
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserDevices>> GetActiveDevicesByUserIdsAsync(List<Guid> userIds)
        {
            return await _context.UserDevices
                .Where(d => d.IsActive && userIds.Contains(d.UserId))
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeactivateDeviceAsync(Guid id)
        {
            var device = await _context.UserDevices.FindAsync(id);
            if (device == null)
                return false;

            device.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAllUserDevicesAsync(Guid userId)
        {
            var devices = await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive)
                .ToListAsync();

            if (!devices.Any())
                return false;

            foreach (var device in devices)
            {
                device.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
