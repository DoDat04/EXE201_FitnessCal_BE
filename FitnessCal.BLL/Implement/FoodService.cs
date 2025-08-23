using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FoodDTO.Response;
using FitnessCal.BLL.Constants;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class FoodService : IFoodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FoodService> _logger;

        public FoodService(IUnitOfWork unitOfWork, ILogger<FoodService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SearchFoodPaginationResponseDTO> SearchFoodsAsync(string? searchTerm = null, int page = 1, int pageSize = 15)
        {
            try
            {
                // Validate page parameters
                page = Math.Max(1, page);
                pageSize = Math.Max(1, Math.Min(pageSize, 50)); // Giới hạn max 50 món/page

                var allResults = new List<SearchFoodResponseDTO>();

                // Search trong bảng Foods (không giới hạn để đếm tổng)
                IEnumerable<Food> allFoods;
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    allFoods = await _unitOfWork.Foods.GetAllAsync();
                }
                else
                {
                    allFoods = await _unitOfWork.Foods.GetAllAsync(food => 
                        food.Name.ToLower().Contains(searchTerm.ToLower()));
                }

                // Search trong bảng PredefinedDishes (không giới hạn để đếm tổng)
                IEnumerable<PredefinedDish> allDishes;
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    allDishes = await _unitOfWork.PredefinedDishes.GetAllAsync();
                }
                else
                {
                    allDishes = await _unitOfWork.PredefinedDishes.GetAllAsync(dish => 
                        dish.Name.ToLower().Contains(searchTerm.ToLower()));
                }

                // Convert tất cả Foods sang DTO
                var allFoodDTOs = allFoods.Select(food => new SearchFoodResponseDTO
                {
                    Id = food.FoodId,
                    Name = food.Name,
                    Calories = food.Calories,
                    Carbs = food.Carbs,
                    Fat = food.Fat,
                    Protein = food.Protein,
                    ServingUnit = null,
                    SourceType = "Food",
                    FoodId = food.FoodId,
                    DishId = null
                });

                // Convert tất cả PredefinedDishes sang DTO
                var allDishDTOs = allDishes.Select(dish => new SearchFoodResponseDTO
                {
                    Id = dish.DishId,
                    Name = dish.Name,
                    Calories = dish.Calories,
                    Carbs = dish.Carbs,
                    Fat = dish.Fat,
                    Protein = dish.Protein,
                    ServingUnit = dish.ServingUnit,
                    SourceType = "PredefinedDish",
                    FoodId = null,
                    DishId = dish.DishId
                });

                // Gộp tất cả kết quả
                allResults.AddRange(allFoodDTOs);
                allResults.AddRange(allDishDTOs);

                // Sắp xếp theo độ liên quan nếu có search term
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    allResults = allResults.OrderBy(r => r.Name.ToLower().IndexOf(searchTerm.ToLower())).ToList();
                }

                // Tính toán pagination
                int totalCount = allResults.Count;
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                int skip = (page - 1) * pageSize;

                // Lấy kết quả cho page hiện tại
                var pageResults = allResults.Skip(skip).Take(pageSize).ToList();

                var response = new SearchFoodPaginationResponseDTO
                {
                    Foods = pageResults,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                _logger.LogInformation("Food search completed. Page {Page}, Total: {TotalCount}, PageSize: {PageSize}, TotalPages: {TotalPages}", 
                    page, totalCount, pageSize, totalPages);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching foods. Page: {Page}, PageSize: {PageSize}", page, pageSize);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }
    }
}
