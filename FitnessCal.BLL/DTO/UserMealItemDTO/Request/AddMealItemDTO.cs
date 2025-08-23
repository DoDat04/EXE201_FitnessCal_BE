using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserMealItemDTO.Request
{
    public class AddMealItemDTO
    {
        public int MealLogId { get; set; }
        public int? FoodId { get; set; }        
        public int? DishId { get; set; }        
        public double Quantity { get; set; }    
    }
}
