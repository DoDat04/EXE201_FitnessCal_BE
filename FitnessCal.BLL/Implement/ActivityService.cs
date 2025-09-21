using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.ActivityDTO.Response;
using FitnessCal.DAL.Define;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(IUnitOfWork unitOfWork, ILogger<ActivityService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<ActivityResponseDTO>> GetAllActivitiesAsync()
    {
        try
        {
            _logger.LogInformation("Getting all activities");

            var activities = await _unitOfWork.Activities.GetAllAsync();
            
            var result = activities.Select(a => new ActivityResponseDTO
            {
                ActivityId = a.ActivityId,
                Name = a.Name,
                DurationMinutes = a.DurationMinutes,
                CaloriesBurned = a.CaloriesBurned
            }).ToList();

            _logger.LogInformation("Retrieved {Count} activities", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all activities");
            throw;
        }
    }
}