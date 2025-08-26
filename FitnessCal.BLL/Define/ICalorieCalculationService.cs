using FitnessCal.BLL.DTO.UserHealthDTO.Request;
using FitnessCal.BLL.DTO.UserHealthDTO.Response;

namespace FitnessCal.BLL.Define;

public interface ICalorieCalculationService
{
    Task<CalculateCaloriesResponseDTO> CalculateDailyCaloriesAsync(CalculateCaloriesRequestDTO request, Guid userId);
    Task<CalculateCaloriesResponseDTO> CalculateDailyCaloriesForUserAsync(Guid userId);
    Task<UpdateUserHealthResponseDTO> UpdateUserHealthAsync(Guid userId, UpdateUserHealthRequestDTO request);
    double CalculateBMR(string gender, DateTime dateOfBirth, decimal height, decimal weight);
    double CalculateTDEE(double bmr, string activityLevel);
    double AdjustCaloriesForGoal(double tdee, string goal, string? intensityLevel);
}
