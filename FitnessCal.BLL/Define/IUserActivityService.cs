using FitnessCal.BLL.DTO.UserActivityDTO.Request;
using FitnessCal.BLL.DTO.UserActivityDTO.Response;

namespace FitnessCal.BLL.Define;

public interface IUserActivityService
{
    Task<List<UserActivityResponseDTO>> GetUserActivitiesAsync(Guid userId, DateOnly? date = null);
    Task<UserActivityResponseDTO> AddUserActivityAsync(Guid userId, AddUserActivityRequestDTO request);
    Task<bool> UpdateUserActivityAsync(Guid userId, int userActivityId, UpdateUserActivityRequestDTO request);
    Task<bool> DeleteUserActivityAsync(Guid userId, int userActivityId);
    Task<int> GetTotalCaloriesBurnedAsync(Guid userId, DateOnly date);
}
