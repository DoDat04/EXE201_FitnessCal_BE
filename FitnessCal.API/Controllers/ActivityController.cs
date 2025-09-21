using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.ActivityDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivityController> _logger;

    public ActivityController(IActivityService activityService, ILogger<ActivityController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ActivityResponseDTO>>>> GetAllActivities()
    {
        try
        {
            _logger.LogInformation("Getting all activities");
            
            var activities = await _activityService.GetAllActivitiesAsync();
            
            return Ok(new ApiResponse<List<ActivityResponseDTO>>
            {
                Success = true,
                Message = activities.Any() ? "Lấy danh sách hoạt động thành công" : "Không có hoạt động nào",
                Data = activities
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all activities");
            return StatusCode(500, new ApiResponse<List<ActivityResponseDTO>>
            {
                Success = false,
                Message = "Có lỗi xảy ra khi lấy danh sách hoạt động",
                Data = null
            });
        }
    }
}