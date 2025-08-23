using System;

namespace FitnessCal.BLL.DTO.FoodDTO.Response
{
    public class SearchFoodResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double Calories { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double Protein { get; set; }
        public string? ServingUnit { get; set; }
        public string SourceType { get; set; } = null!; // "Food" hoặc "PredefinedDish"
        public int? FoodId { get; set; } // null nếu là PredefinedDish
        public int? DishId { get; set; } // null nếu là Food
    }

    public class SearchFoodPaginationResponseDTO
    {
        public IEnumerable<SearchFoodResponseDTO> Foods { get; set; } = new List<SearchFoodResponseDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
