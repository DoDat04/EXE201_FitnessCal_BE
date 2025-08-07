using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserMealItemDTO.Response
{
    public class AddMealItemResponseDTO
    {
        public int ItemId { get; set; }
        public int MealLogId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public double Calories { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
