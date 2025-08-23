using System;

namespace FitnessCal.BLL.DTO.UserHealthDTO.Request;

public class CalculateCaloriesRequestDTO
{
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public decimal? TargetWeightKg { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string? IntensityLevel { get; set; }
    public string? DietType { get; set; }
}
