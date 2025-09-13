namespace FitnessCal.BLL.DTO.AllergyDTO.Response
{
    public class AllergyResponseDTO
    {
        public int AllergyId { get; set; }
        public Guid UserId { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
