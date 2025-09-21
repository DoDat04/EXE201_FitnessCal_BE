namespace FitnessCal.BLL.DTO.ActivityDTO.Response;

public class ActivityResponseDTO
{
    public int ActivityId { get; set; }
    public string Name { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
}