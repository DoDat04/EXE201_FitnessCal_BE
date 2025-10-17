namespace FitnessCal.BLL.DTO.FoodDTO.Response;

public class ConfirmCapturedFoodResponseDTO
{
    public int CapturedFoodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Protein { get; set; }
}


