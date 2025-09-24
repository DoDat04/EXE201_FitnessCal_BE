namespace FitnessCal.BLL.DTO.AllergyDTO.Response
{
    public class AllergyResponseDTO
    {
        public int AllergyId { get; set; }
        public Guid UserId { get; set; }
        public int? FoodId { get; set; }
        public string? FoodName { get; set; }
        public int? DishId { get; set; }
        public string? DishName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
