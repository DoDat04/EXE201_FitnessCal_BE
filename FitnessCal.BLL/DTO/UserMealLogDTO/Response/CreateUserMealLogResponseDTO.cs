using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserMealLogDTO.Response
{
    public class CreateUserMealLogResponseDTO
    {
        public Guid UserId { get; set; }
        public DateOnly MealDate { get; set; }
        public List<int> MealLogIds { get; set; } = new List<int>(); // [Breakfast, Lunch, Dinner]
        public string Message { get; set; } = string.Empty;
    }
}
