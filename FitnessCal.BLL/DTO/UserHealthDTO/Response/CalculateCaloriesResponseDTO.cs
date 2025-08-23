namespace FitnessCal.BLL.DTO.UserHealthDTO.Response;

public class CalculateCaloriesResponseDTO
{
    public double BMR { get; set; }
    public double TDEE { get; set; }
    public double DailyCalories { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public DateTime? EstimatedGoalDate { get; set; }
    public int EstimatedWeeks { get; set; }
    public string DietRecommendation { get; set; } = string.Empty;
    public string GoalNote { get; set; } = string.Empty;
}
