namespace FitnessCal.BLL.DTO.UserMealItemDTO.Response
{
    public class DeleteMealItemResponseDTO
    {
        public int ItemId { get; set; }
        public int MealLogId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
