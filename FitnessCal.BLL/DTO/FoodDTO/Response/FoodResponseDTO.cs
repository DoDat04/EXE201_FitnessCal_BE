using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.FoodDTO.Response
{
    public class FoodResponseDTO
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double Protein { get; set; }
    }
}
