using FitnessCal.BLL.Helpers;

namespace FitnessCal.BLL.DTO.UserHealthDTO.Response;

public class CalculateCaloriesResponseDTO
{
    public double BMR { get; set; }
    public double TDEE { get; set; }
    public double DailyCalories { get; set; }
    
    /// <summary>
    /// Macro targets (protein, carbs, fat) based on daily calories
    /// </summary>
    public MacroTargetDTO MacroTarget { get; set; } = new();
    
    public string Explanation { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public DateTime? EstimatedGoalDate { get; set; }
    public int EstimatedWeeks { get; set; }
    public string DietRecommendation { get; set; } = string.Empty;
    public string GoalNote { get; set; } = string.Empty;

    // UserHealth fields (echo back for client convenience)
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TargetWeightKg { get; set; }
    public string? ActivityLevel { get; set; }
    public string? Goal { get; set; }
    public string? DietType { get; set; }
    public string? IntensityLevel { get; set; }
}
