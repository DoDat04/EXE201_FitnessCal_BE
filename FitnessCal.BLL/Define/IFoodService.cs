using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.FoodDTO.Request;
using FitnessCal.BLL.DTO.FoodDTO.Response;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.BLL.Define
{
    public interface IFoodService
    {
        Task<SearchFoodPaginationResponseDTO> SearchFoodsAsync(string? searchTerm = null, int page = 1, int pageSize = 15);
        Task<string> GenerateFoodsInformationAsync(string prompt);
        Task<ApiResponse<object>> UploadAndDetectFood(UploadFileRequest request, string prompt);
    }
}
