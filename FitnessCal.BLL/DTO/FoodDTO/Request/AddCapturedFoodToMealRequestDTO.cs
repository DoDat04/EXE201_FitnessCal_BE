using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.FoodDTO.Request
{
    public class AddCapturedFoodToMealRequestDTO
    {
        [Required(ErrorMessage = "CapturedFoodId là bắt buộc")]
        public int CapturedFoodId { get; set; }

        [Required(ErrorMessage = "MealLogId là bắt buộc")]
        public int MealLogId { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public double Quantity { get; set; } = 1.0;
    }
}
