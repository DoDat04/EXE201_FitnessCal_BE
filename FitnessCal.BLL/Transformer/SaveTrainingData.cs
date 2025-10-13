using FitnessCal.BLL.Implement;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Tools;

public class SaveTrainingData
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClassifyData _classifyData;
    private readonly ILogger<FoodService> _logger;

    public SaveTrainingData(IUnitOfWork unitOfWork, ClassifyData classifyData, ILogger<FoodService> logger)
    {
        _unitOfWork = unitOfWork;
        _classifyData = classifyData;
        _logger = logger;
    }

    protected internal async Task SaveTrainingDataAsync(FoodService.ParsedFoodInfo foodInfo)
    {
        if (foodInfo == null || string.IsNullOrWhiteSpace(foodInfo.Name))
            return;

        // 1. Phân loại ingredient hay dish
        var type = await _classifyData.ClassifyFoodOrDishAsync(foodInfo.Name);

        if (type == "Food")
        {
            // Kiểm tra đã tồn tại chưa
            var existingFood = await _unitOfWork.Foods
                .FirstOrDefaultAsync(f => f.Name.ToLower() == foodInfo.Name.ToLower());

            if (existingFood == null)
            {
                var newFood = new Food
                {
                    Name = foodInfo.Name,
                    Calories = foodInfo.Calories,
                    Carbs = foodInfo.Carbs,
                    Fat = foodInfo.Fat,
                    Protein = foodInfo.Protein,
                    FoodCategory = null // có thể dùng AI classify thêm
                };
                await _unitOfWork.Foods.AddAsync(newFood);
                await _unitOfWork.Save();

                _logger.LogInformation($"Đã lưu nguyên liệu '{newFood.Name}' vào bảng Foods.");
            }
            else
            {
                _logger.LogInformation($"Nguyên liệu '{foodInfo.Name}' đã tồn tại.");
            }
        }
        else // PredefinedDish
        {
            var existingDish = await _unitOfWork.PredefinedDishes
                .FirstOrDefaultAsync(d => d.Name.ToLower() == foodInfo.Name.ToLower());

            if (existingDish == null)
            {
                var newDish = new PredefinedDish
                {
                    Name = foodInfo.Name,
                    Calories = foodInfo.Calories,
                    Carbs = foodInfo.Carbs,
                    Fat = foodInfo.Fat,
                    Protein = foodInfo.Protein,
                    ServingUnit = "1 phần"
                };
                await _unitOfWork.PredefinedDishes.AddAsync(newDish);
                await _unitOfWork.Save();

                _logger.LogInformation($"Đã lưu món ăn '{newDish.Name}' vào bảng PredefinedDish.");
            }
            else
            {
                _logger.LogInformation($"Món ăn '{foodInfo.Name}' đã tồn tại.");
            }
        }
    }
}