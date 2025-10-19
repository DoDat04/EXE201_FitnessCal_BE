using FitnessCal.BLL.DTO.UserDTO.Response;
using FitnessCal.BLL.DTO.DashboardDTO.Response;
using FitnessCal.BLL.DTO.UserHealthDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO?> GetUserByIdAsync(Guid userId);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> UnBanUserAsync(Guid userId);
        Task<UserStatisticsDTO> GetUserStatisticsAsync();

        Task<byte[]> ExportUsersPremiumToExcelAsync();

        Task<IEnumerable<UserResponseDTO>> GetUsersWithoutMealLogAsync(DateOnly today);

        Task<RevenueStatisticsDTO> GetRevenueStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        Task<ConversionRateResponseDTO> GetConversionRateAsync();

    }
}
