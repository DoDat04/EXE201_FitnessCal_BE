using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Define
{
    public interface IOTPService
    {
        Task<ApiResponse<bool>> SendOTPAsync(string email, string purpose);
        Task<ApiResponse<bool>> VerifyOTPAsync(string email, string otpCode, string purpose);
        Task<ApiResponse<bool>> ResendOTPAsync(string email, string purpose);
        Task CleanupExpiredOTPsAsync();
        Task CleanupUsedOTPsAsync();

    }
}
