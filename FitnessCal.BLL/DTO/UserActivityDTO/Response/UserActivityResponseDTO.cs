namespace FitnessCal.BLL.DTO.UserActivityDTO.Response;

public class UserActivityResponseDTO
{
    public int UserActivityId { get; set; }
    public Guid UserId { get; set; }
    public int ActivityId { get; set; }
    public string ActivityName { get; set; } = null!;
    public int ActivityDurationMinutes { get; set; }  // Duration từ Activity (30 phút)
    public int UserDurationMinutes { get; set; }      // Duration mà user thực hiện
    public int CaloriesBurned { get; set; }           // Calories thực tế đốt cháy
    public DateOnly ActivityDate { get; set; }
}
