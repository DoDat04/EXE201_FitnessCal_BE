using FitnessCal.BLL.DTO.UserDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;

namespace FitnessCal.BLL.Define
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> UnBanUserAsync(Guid userId);
        Task<UserStatisticsDTO> GetUserStatisticsAsync();
        Task<IEnumerable<UserResponseDTO>> GetUsersWithoutMealLogAsync(DateOnly today);
    }
}
