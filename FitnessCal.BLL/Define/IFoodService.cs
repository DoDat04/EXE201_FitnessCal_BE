using FitnessCal.BLL.DTO.FoodDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IFoodService
    {
        Task<SearchFoodPaginationResponseDTO> SearchFoodsAsync(string? searchTerm = null, int page = 1, int pageSize = 15);
        Task<string> GenerateFoodsInformationAsync(string prompt);
    }
}
