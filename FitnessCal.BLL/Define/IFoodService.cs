using FitnessCal.BLL.DTO.FoodDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IFoodService
    {
        Task<IEnumerable<FoodResponseDTO>> GetFoodsAsync(string? searchTerm = null);
    }
}
