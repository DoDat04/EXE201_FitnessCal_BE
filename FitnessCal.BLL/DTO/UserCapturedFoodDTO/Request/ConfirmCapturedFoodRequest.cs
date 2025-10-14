using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserCapturedFoodDTO.Request
{
    public class ConfirmCapturedFoodRequest
    {
        public string Name { get; set; } = null!;

        public double Calories { get; set; }

        public double Carbs { get; set; }

        public double Fat { get; set; }

        public double Protein { get; set; }

        public string ImageUrl { get; set; } = null!;
    }
}
