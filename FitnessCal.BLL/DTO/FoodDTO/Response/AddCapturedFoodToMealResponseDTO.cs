namespace FitnessCal.BLL.DTO.FoodDTO.Response
{
    public class AddCapturedFoodToMealResponseDTO
    {
        public int ItemId { get; set; }
        public int MealLogId { get; set; }
        public int CapturedFoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public double Calories { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
