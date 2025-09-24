namespace FitnessCal.BLL.DTO.FavoriteFoodDTO.Response
{
    public class FavoriteFoodResponseDTO
    {
        public int FavoriteId { get; set; }
        public Guid UserId { get; set; }
        public int? FoodId { get; set; }
        public string? FoodName { get; set; }
        public int? DishId { get; set; }
        public string? DishName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
