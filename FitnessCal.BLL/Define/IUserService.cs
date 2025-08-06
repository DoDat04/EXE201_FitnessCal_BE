using FitnessCal.BLL.DTO.UserDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> UnBanUserAsync(Guid userId);
    }
}
