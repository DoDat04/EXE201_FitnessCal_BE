using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserMealItemDTO.Response
{
    public class UpdateMealItemResponseDTO
    {
        public int ItemId { get; set; }
        public int MealLogId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public double OldQuantity { get; set; }
        public double NewQuantity { get; set; }
        public double OldCalories { get; set; }
        public double NewCalories { get; set; }
    }
}
