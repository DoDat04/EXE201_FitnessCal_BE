using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.FoodDTO.Request;
using FitnessCal.BLL.DTO.FoodDTO.Response;
using FitnessCal.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.BLL.Define
{
    public interface IFoodService
    {
        Task<SearchFoodPaginationResponseDTO> SearchFoodsAsync(string? searchTerm = null, int page = 1, int pageSize = 15);
        Task<List<AddFoodResponseDTO>> AddFoodInformationAsync(List<AddFoodRequestDTO> foods);
        Task<IEnumerable<Food?>> SearchFoodByNameAsync(string name);
        Task<IEnumerable<PredefinedDish?>> SearchPredefinedDishByNameAsync(string name);
        Task<string> GenerateFoodsInformationAsync(string prompt);
        Task<ApiResponse<object>> UploadAndDetectFood(UploadFileRequest request);
        Task<ApiResponse<ConfirmCapturedFoodResponseDTO>> ConfirmCapturedFoodAsync(ConfirmCapturedFoodRequestDTO request);
        Task<SearchFoodResponseDTO> GetFoodDetailsAsync(int id, string type);
        Task<AddCapturedFoodToMealResponseDTO> AddCapturedFoodToMealAsync(AddCapturedFoodToMealRequestDTO request);
        Task<List<GetUserCapturedFoodsResponseDTO>> GetUserCapturedFoodsAsync();
    }
}
