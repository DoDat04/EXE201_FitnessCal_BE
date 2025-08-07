using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserMealLogDTO.Request
{
    public class CreateUserMealLogDTO
    {
        public Guid? UserId { get; set; }
        public DateOnly MealDate { get; set; }
    }
}
