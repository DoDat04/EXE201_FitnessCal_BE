using FitnessCal.BLL.DTO.CommonDTO;

namespace FitnessCal.BLL.Define
{
    public interface IEmailService
    {
        Task<ApiResponse<bool>> SendEmailAsync(string to, string subject, string htmlContent);
        Task<ApiResponse<bool>> GuestSendEmailAsync(string from, string subject, string htmlContent);
    }
}
