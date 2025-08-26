namespace FitnessCal.BLL.DTO.UserHealthDTO.Response;

public class UpdateUserHealthResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public double? DailyCalories { get; set; }
    public string Goal { get; set; } = string.Empty;
}


