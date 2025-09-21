namespace FitnessCal.BLL.DTO.UserActivityDTO.Request;

public class AddUserActivityRequestDTO
{
    public int ActivityId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public int DurationMinutes { get; set; }
}

public class UpdateUserActivityRequestDTO
{
    public int DurationMinutes { get; set; }
}
