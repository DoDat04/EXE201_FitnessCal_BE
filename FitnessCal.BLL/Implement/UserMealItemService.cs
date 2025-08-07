using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealItemDTO.Request;
using FitnessCal.BLL.DTO.UserMealItemDTO.Response;
using FitnessCal.BLL.Constants;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Implement
{
    public class UserMealItemService : IUserMealItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserMealItemService> _logger;

        public UserMealItemService(IUnitOfWork unitOfWork, ILogger<UserMealItemService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AddMealItemResponseDTO> AddMealItemAsync(AddMealItemDTO dto)
        {
            try
            {
                if (dto.MealLogId <= 0)
                {
                    _logger.LogWarning("Invalid MealLogId provided: {MealLogId}", dto.MealLogId);
                    throw new ArgumentException("MealLogId không hợp lệ");
                }

                if (dto.Quantity <= 0)
                {
                    _logger.LogWarning("Invalid Quantity provided: {Quantity}", dto.Quantity);
                    throw new ArgumentException("Số lượng phải lớn hơn 0");
                }

                var mealLog = await _unitOfWork.UserMealLogs.GetByIdAsync(dto.MealLogId);
                if (mealLog == null)
                {
                    _logger.LogWarning("Meal log with ID {MealLogId} not found", dto.MealLogId);
                    throw new KeyNotFoundException("Không tìm thấy bữa ăn");
                }

                string foodName = string.Empty;
                double calories = 0;
                bool isCustom = false;

                if (dto.FoodId.HasValue)
                {
                    // Food from database
                    var food = await _unitOfWork.Foods.GetByIdAsync(dto.FoodId.Value);
                    if (food == null)
                    {
                        _logger.LogWarning("Food with ID {FoodId} not found", dto.FoodId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn");
                    }

                    foodName = food.Name;
                    calories = food.Calories * dto.Quantity;
                    isCustom = false;
                }
                else if (dto.DishId.HasValue)
                {
                    // Predefined dish
                    var dish = await _unitOfWork.PredefinedDishes.GetByIdAsync(dto.DishId.Value);
                    if (dish == null)
                    {
                        _logger.LogWarning("Dish with ID {DishId} not found", dto.DishId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn định sẵn");
                    }

                    foodName = dish.Name;
                    calories = dish.Calories * dto.Quantity;
                    isCustom = false;
                }
                else
                {
                    _logger.LogWarning("No food source specified");
                    throw new ArgumentException("Phải chọn món ăn từ database hoặc món ăn định sẵn");
                }

                // Create new meal item
                var mealItem = new UserMealItem
                {
                    LogId = dto.MealLogId,
                    IsCustom = isCustom,
                    FoodId = dto.FoodId,
                    DishId = dto.DishId,
                    Quantity = dto.Quantity,
                    Calories = calories
                };

                await _unitOfWork.UserMealItems.AddAsync(mealItem);
                var result = await _unitOfWork.Save();

                if (result)
                {
                    _logger.LogInformation("Meal item added successfully: {FoodName} to meal log {MealLogId}", 
                        foodName, dto.MealLogId);
                }

                return new AddMealItemResponseDTO
                {
                    ItemId = mealItem.ItemId,
                    MealLogId = dto.MealLogId,
                    FoodName = foodName,
                    Quantity = dto.Quantity,
                    Calories = calories,
                    Message = "Thêm món ăn thành công"
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding meal item to meal log {MealLogId}", dto.MealLogId);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }
    }
}
