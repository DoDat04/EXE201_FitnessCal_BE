using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserActivityDTO.Request;
using FitnessCal.BLL.DTO.UserActivityDTO.Response;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement;

public class UserActivityService : IUserActivityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(IUnitOfWork unitOfWork, ILogger<UserActivityService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<UserActivityResponseDTO>> GetUserActivitiesAsync(Guid userId, DateOnly? date = null)
    {
        try
        {
            _logger.LogInformation("Getting user activities for user {UserId}, date: {Date}", userId, date);

            IEnumerable<UserActivity> userActivities;
            
            if (date.HasValue)
            {
                userActivities = await _unitOfWork.UserActivities.GetAllAsync(ua => ua.UserId == userId && ua.ActivityDate == date.Value, ua => ua.Activity);
            }
            else
            {
                userActivities = await _unitOfWork.UserActivities.GetAllAsync(ua => ua.UserId == userId, ua => ua.Activity);
            }
            
            var result = userActivities.Select(ua => new UserActivityResponseDTO
            {
                UserActivityId = ua.UserActivityId,
                UserId = ua.UserId,
                ActivityId = ua.ActivityId,
                ActivityName = ua.Activity.Name,
                ActivityDurationMinutes = ua.Activity.DurationMinutes,
                UserDurationMinutes = ua.DurationMinutes,
                CaloriesBurned = CalculateCaloriesBurned(ua.Activity.CaloriesBurned, ua.Activity.DurationMinutes, ua.DurationMinutes),
                ActivityDate = ua.ActivityDate
            }).ToList();

            _logger.LogInformation("Retrieved {Count} user activities", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user activities for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserActivityResponseDTO> AddUserActivityAsync(Guid userId, AddUserActivityRequestDTO request)
    {
        try
        {
            _logger.LogInformation("Adding user activity for user {UserId}, activity {ActivityId}, date {Date}", 
                userId, request.ActivityId, request.ActivityDate);

            // Kiểm tra activity có tồn tại không
            var activity = await _unitOfWork.Activities.GetByIdAsync(request.ActivityId);
            if (activity == null)
            {
                throw new ArgumentException($"Activity with ID {request.ActivityId} not found");
            }

            // Kiểm tra duplicate
            var existingActivity = await _unitOfWork.UserActivities.GetAllAsync(ua => 
                ua.UserId == userId && 
                ua.ActivityId == request.ActivityId && 
                ua.ActivityDate == request.ActivityDate);

            if (existingActivity.Any())
            {
                throw new InvalidOperationException("User has already added this activity for this date");
            }

            var userActivity = new UserActivity
            {
                UserId = userId,
                ActivityId = request.ActivityId,
                ActivityDate = request.ActivityDate,
                DurationMinutes = request.DurationMinutes
            };

            await _unitOfWork.UserActivities.AddAsync(userActivity);
            await _unitOfWork.Save();

            var result = new UserActivityResponseDTO
            {
                UserActivityId = userActivity.UserActivityId,
                UserId = userActivity.UserId,
                ActivityId = userActivity.ActivityId,
                ActivityName = activity.Name,
                ActivityDurationMinutes = activity.DurationMinutes,
                UserDurationMinutes = userActivity.DurationMinutes,
                CaloriesBurned = CalculateCaloriesBurned(activity.CaloriesBurned, activity.DurationMinutes, userActivity.DurationMinutes),
                ActivityDate = userActivity.ActivityDate
            };

            _logger.LogInformation("Successfully added user activity: {ActivityName}", activity.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding user activity for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateUserActivityAsync(Guid userId, int userActivityId, UpdateUserActivityRequestDTO request)
    {
        try
        {
            _logger.LogInformation("Updating user activity {UserActivityId} for user {UserId}", userActivityId, userId);

            var userActivity = await _unitOfWork.UserActivities.GetByIdAsync(userActivityId);
            if (userActivity == null || userActivity.UserId != userId)
            {
                _logger.LogWarning("User activity {UserActivityId} not found or not owned by user {UserId}", userActivityId, userId);
                return false;
            }

            userActivity.DurationMinutes = request.DurationMinutes;

            await _unitOfWork.UserActivities.UpdateAsync(userActivity);
            await _unitOfWork.Save();

            _logger.LogInformation("Successfully updated user activity {UserActivityId} duration to {DurationMinutes} minutes", userActivityId, request.DurationMinutes);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user activity {UserActivityId} for user {UserId}", userActivityId, userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserActivityAsync(Guid userId, int userActivityId)
    {
        try
        {
            _logger.LogInformation("Deleting user activity {UserActivityId} for user {UserId}", userActivityId, userId);

            var userActivity = await _unitOfWork.UserActivities.GetByIdAsync(userActivityId);
            if (userActivity == null || userActivity.UserId != userId)
            {
                _logger.LogWarning("User activity {UserActivityId} not found or not owned by user {UserId}", userActivityId, userId);
                return false;
            }

            await _unitOfWork.UserActivities.DeleteAsync(userActivity);
            await _unitOfWork.Save();

            _logger.LogInformation("Successfully deleted user activity {UserActivityId}", userActivityId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user activity {UserActivityId} for user {UserId}", userActivityId, userId);
            throw;
        }
    }

    public async Task<int> GetTotalCaloriesBurnedAsync(Guid userId, DateOnly date)
    {
        try
        {
            _logger.LogInformation("Getting total calories burned for user {UserId} on date {Date}", userId, date);

            var userActivities = await _unitOfWork.UserActivities.GetAllAsync(ua => 
                ua.UserId == userId && ua.ActivityDate == date, ua => ua.Activity);

            var totalCalories = userActivities.Sum(ua => 
                CalculateCaloriesBurned(ua.Activity.CaloriesBurned, ua.Activity.DurationMinutes, ua.DurationMinutes));

            _logger.LogInformation("Total calories burned for user {UserId} on {Date}: {Calories}", userId, date, totalCalories);
            return totalCalories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting total calories burned for user {UserId} on date {Date}", userId, date);
            throw;
        }
    }

    private int CalculateCaloriesBurned(int activityCalories, int activityDurationMinutes, int userDurationMinutes)
    {
        // Tính calories theo tỷ lệ thời gian thực hiện
        // Ví dụ: Activity = 300 cal/30 phút, User thực hiện 45 phút
        // Calories = (45 / 30) * 300 = 1.5 * 300 = 450 cal
        return (int)Math.Round((double)userDurationMinutes / activityDurationMinutes * activityCalories);
    }
}
