using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.MealPlanningDTO
{
    public class MealPlanningRequestDTO
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
