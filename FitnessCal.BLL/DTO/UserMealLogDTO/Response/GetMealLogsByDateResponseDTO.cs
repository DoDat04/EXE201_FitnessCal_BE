using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserMealLogDTO.Response
{
    public class GetMealLogsByDateResponseDTO
    {
        public Guid UserId { get; set; }
        public DateOnly MealDate { get; set; }
        public List<MealLogSummaryDTO> MealLogs { get; set; } = new List<MealLogSummaryDTO>();
    }

    public class MealLogSummaryDTO
    {
        public int LogId { get; set; }
        public string MealType { get; set; } = string.Empty;
        public double TotalCalories { get; set; }
        public double TargetCalories { get; set; }
        public List<MealItemDTO> Items { get; set; } = new List<MealItemDTO>();
    }

    public class MealItemDTO
    {
        public int ItemId { get; set; }
        public int? FoodId { get; set; }
        public int? DishId { get; set; }
        public int? UserCapturedFoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public double Calories { get; set; }
    }
}
