using FitnessCal.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.FoodDTO.Request
{
    public class AddFoodRequestDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;

        [Range(0.1, double.MaxValue, ErrorMessage = "Calories must be a non-negative value.")]
        public double Calories { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Carbs must be a non-negative value.")]
        public double Carbs { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Fat must be a non-negative value.")]
        public double Fat { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Protein must be a non-negative value.")]
        public double Protein { get; set; }

        public string? FoodCategory { get; set; }
    }
}
