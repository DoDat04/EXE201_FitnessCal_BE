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

        public async Task<IEnumerable<FoodResponseDTO>> GetFoodsAsync(string? searchTerm = null)
        {
            try
            {
                IEnumerable<Food> foods;

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    foods = await _unitOfWork.Foods.GetAllAsync();
                    _logger.LogInformation("Retrieving all foods");
                }
                else
                {
                    foods = await _unitOfWork.Foods.GetAllAsync(food => 
                        food.Name.ToLower().Contains(searchTerm.ToLower()));
                    _logger.LogInformation("Searching foods with term '{SearchTerm}'", searchTerm);
                }

                var foodDTOs = foods.Select(food => new FoodResponseDTO
                {
                    FoodId = food.FoodId,
                    Name = food.Name,
                    Calories = food.Calories,
                    Carbs = food.Carbs,
                    Fat = food.Fat,
                    Protein = food.Protein
                }).ToList();

                _logger.LogInformation("Retrieved {Count} foods", foodDTOs.Count);
                return foodDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving foods with searchTerm: {SearchTerm}", searchTerm);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }
    }
}
