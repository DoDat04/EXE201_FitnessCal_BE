namespace FitnessCal.BLL.DTO.MealPlanningDTO
{
    public class MealPlanningResponseDTO
    {
        public Guid UserId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public NutritionTargetDTO DailyTarget { get; set; } = new();
        public NutritionActualDTO ActualDaily { get; set; } = new();
        public List<MealDTO> Meals { get; set; } = new();
    }

    public class NutritionTargetDTO
    {
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
    }

    public class NutritionActualDTO
    {
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
    }

    public class MealDTO
    {
        public string MealType { get; set; } = string.Empty;
        public string MealName { get; set; } = string.Empty;
        public double TargetCalories { get; set; }
        public double ActualCalories { get; set; }
        public List<MealFoodDTO> Foods { get; set; } = new();
        public NutritionInfoDTO MealNutrition { get; set; } = new();
    }

    public class MealFoodDTO
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public NutritionInfoDTO BaseNutrition { get; set; } = new();
        public NutritionInfoDTO CalculatedNutrition { get; set; } = new();
    }

    public class NutritionInfoDTO
    {
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
    }
}
