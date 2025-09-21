using FitnessCal.BLL.DTO.ActivityDTO.Response;

namespace FitnessCal.BLL.Define;

public interface IActivityService
{
    Task<List<ActivityResponseDTO>> GetAllActivitiesAsync();
}