using FitnessCal.BLL.DTO.UserMealLogDTO.Request;
using FitnessCal.BLL.DTO.UserMealLogDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IUserMealLogService
    {
        Task<CreateUserMealLogResponseDTO> AutoCreateMealLogsAsync(Guid userId, CreateUserMealLogDTO dto);
        Task<GetMealLogsByDateResponseDTO> GetMealLogsByDateAsync(Guid userId, DateOnly date);
    }
}
