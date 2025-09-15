using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IOTPRepository : IGenericRepository<OTP>
    {
        Task<OTP?> GetValidOTPAsync(string email, string otpCode, string purpose);
        Task<List<OTP>> GetExpiredOTPsAsync();
        Task<bool> InvalidateOTPsAsync(string email, string purpose);
        Task<int> GetOTPCountByEmailAsync(string email, string purpose, DateTime fromTime);
        Task<List<OTP>> GetUsedOTPsAsync();
        void DeleteOTPs(IEnumerable<OTP> otps);

    }
}
