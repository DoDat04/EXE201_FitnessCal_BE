using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement
{
    public class OTPRepository : GenericRepository<OTP>, IOTPRepository
    {
        private readonly FitnessCalContext _fitnessCalContext;

        public OTPRepository(FitnessCalContext context) : base(context)
        {
            _fitnessCalContext = context;
        }

        public async Task<OTP?> GetValidOTPAsync(string email, string otpCode, string purpose)
        {
            return await _fitnessCalContext.OTPs
                .FirstOrDefaultAsync(otp => 
                    otp.Email == email && 
                    otp.OTPCode == otpCode && 
                    otp.Purpose == purpose && 
                    otp.ExpiresAt > DateTime.UtcNow && 
                    !otp.IsUsed);
        }

        public async Task<List<OTP>> GetExpiredOTPsAsync()
        {
            return await _fitnessCalContext.OTPs
                .Where(otp => otp.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<bool> InvalidateOTPsAsync(string email, string purpose)
        {
            var otps = await _fitnessCalContext.OTPs
                .Where(otp => otp.Email == email && otp.Purpose == purpose && !otp.IsUsed)
                .ToListAsync();

            foreach (var otp in otps)
            {
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
            }

            return await _fitnessCalContext.SaveChangesAsync() > 0;
        }

        public async Task<int> GetOTPCountByEmailAsync(string email, string purpose, DateTime fromTime)
        {
            return await _fitnessCalContext.OTPs
                .CountAsync(otp => 
                    otp.Email == email && 
                    otp.Purpose == purpose && 
                    otp.CreatedAt >= fromTime);
        }
    }
}
