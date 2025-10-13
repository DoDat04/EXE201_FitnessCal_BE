using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealItemDTO.Request;
using FitnessCal.BLL.DTO.UserMealItemDTO.Response;
using FitnessCal.BLL.Constants;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

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

                // Kiểm tra chỉ có một source được chọn
                var sourceCount = 0;
                if (dto.FoodId.HasValue) sourceCount++;
                if (dto.DishId.HasValue) sourceCount++;
                if (dto.UserCapturedFoodId.HasValue) sourceCount++;

                if (sourceCount != 1)
                {
                    _logger.LogWarning("Exactly one source must be provided. FoodId: {FoodId}, DishId: {DishId}, UserCapturedFoodId: {UserCapturedFoodId}", 
                        dto.FoodId, dto.DishId, dto.UserCapturedFoodId);
                    throw new ArgumentException("Chỉ chọn một trong ba: FoodId, DishId hoặc UserCapturedFoodId");
                }

                var mealLog = await _unitOfWork.UserMealLogs.GetByIdAsync(dto.MealLogId);
                if (mealLog == null)
                {
                    _logger.LogWarning("Meal log with ID {MealLogId} not found", dto.MealLogId);
                    throw new KeyNotFoundException("Không tìm thấy bữa ăn");
                }

                string foodName = string.Empty;
                double calories = 0;
                short isCustom = 0;

                if (dto.FoodId.HasValue)
                {
                    var food = await _unitOfWork.Foods.GetByIdAsync(dto.FoodId.Value);
                    if (food == null)
                    {
                        _logger.LogWarning("Food with ID {FoodId} not found", dto.FoodId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn");
                    }

                    foodName = food.Name;
                    calories = food.Calories * dto.Quantity;
                    isCustom = 0;
                }
                else if (dto.DishId.HasValue)
                {
                    var dish = await _unitOfWork.PredefinedDishes.GetByIdAsync(dto.DishId.Value);
                    if (dish == null)
                    {
                        _logger.LogWarning("Dish with ID {DishId} not found", dto.DishId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn định sẵn");
                    }

                    foodName = dish.Name;
                    calories = dish.Calories * dto.Quantity;
                    isCustom = 0;
                }
                else if (dto.UserCapturedFoodId.HasValue)
                {
                    var userCapturedFood = await _unitOfWork.UserCapturedFoods.GetByIdAsync(dto.UserCapturedFoodId.Value);
                    if (userCapturedFood == null)
                    {
                        _logger.LogWarning("UserCapturedFood with ID {UserCapturedFoodId} not found", dto.UserCapturedFoodId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn đã chụp");
                    }

                    foodName = userCapturedFood.Name;
                    calories = userCapturedFood.Calories * dto.Quantity;
                    isCustom = 0;
                }
                else
                {
                    _logger.LogWarning("No food source specified");
                    throw new ArgumentException("Phải chọn món ăn từ database, món ăn định sẵn hoặc món ăn đã chụp");
                }

                var mealItem = new UserMealItem
                {
                    LogId = dto.MealLogId,
                    IsCustom = isCustom,
                    // Chỉ set một FK để tránh vi phạm ràng buộc
                    FoodId = dto.FoodId.HasValue ? dto.FoodId : null,
                    DishId = dto.DishId.HasValue ? dto.DishId : null,
                    UserCapturedFoodId = dto.UserCapturedFoodId.HasValue ? dto.UserCapturedFoodId : null,
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

        public async Task<DeleteMealItemResponseDTO> DeleteMealItemAsync(int itemId)
        {
            try
            {
                if (itemId <= 0)
                {
                    _logger.LogWarning("Invalid ItemId provided: {ItemId}", itemId);
                    throw new ArgumentException("ItemId không hợp lệ");
                }

                var mealItem = await _unitOfWork.UserMealItems.GetByIdAsync(itemId);
                if (mealItem == null)
                {
                    _logger.LogWarning("Meal item with ID {ItemId} not found", itemId);
                    throw new KeyNotFoundException("Không tìm thấy món ăn");
                }

                string foodName = "Unknown";
                if (mealItem.FoodId.HasValue)
                {
                    var food = await _unitOfWork.Foods.GetByIdAsync(mealItem.FoodId.Value);
                    if (food != null)
                    {
                        foodName = food.Name;
                    }
                }
                else if (mealItem.DishId.HasValue)
                {
                    var dish = await _unitOfWork.PredefinedDishes.GetByIdAsync(mealItem.DishId.Value);
                    if (dish != null)
                    {
                        foodName = dish.Name;
                    }
                }
                else if (mealItem.UserCapturedFoodId.HasValue)
                {
                    var userCapturedFood = await _unitOfWork.UserCapturedFoods.GetByIdAsync(mealItem.UserCapturedFoodId.Value);
                    if (userCapturedFood != null)
                    {
                        foodName = userCapturedFood.Name;
                    }
                }

                var mealLogId = mealItem.LogId ?? 0;

                await _unitOfWork.UserMealItems.DeleteAsync(mealItem);
                var result = await _unitOfWork.Save();

                if (result)
                {
                    _logger.LogInformation("Meal item deleted successfully: {FoodName} (ID: {ItemId})", foodName, itemId);
                }

                return new DeleteMealItemResponseDTO
                {
                    ItemId = itemId,
                    MealLogId = mealLogId,
                    FoodName = foodName,
                    Message = "Xóa món ăn thành công"
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
                _logger.LogError(ex, "Error occurred while deleting meal item {ItemId}", itemId);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<UpdateMealItemResponseDTO> UpdateMealItemAsync(int itemId, UpdateMealItemDTO dto)
        {
            try
            {
                if (itemId <= 0)
                {
                    _logger.LogWarning("Invalid ItemId provided: {ItemId}", itemId);
                    throw new ArgumentException("ItemId không hợp lệ");
                }

                if (dto.Quantity <= 0)
                {
                    _logger.LogWarning("Invalid Quantity provided: {Quantity}", dto.Quantity);
                    throw new ArgumentException("Số lượng phải lớn hơn 0");
                }

                var mealItem = await _unitOfWork.UserMealItems.GetByIdAsync(itemId);
                if (mealItem == null)
                {
                    _logger.LogWarning("Meal item with ID {ItemId} not found", itemId);
                    throw new KeyNotFoundException("Không tìm thấy món ăn");
                }

                var oldQuantity = mealItem.Quantity;
                var oldCalories = mealItem.Calories ?? 0;

                double newCalories = 0;
                string foodName = "Unknown";

                if (mealItem.FoodId.HasValue)
                {
                    var food = await _unitOfWork.Foods.GetByIdAsync(mealItem.FoodId.Value);
                    if (food == null)
                    {
                        _logger.LogWarning("Food with ID {FoodId} not found", mealItem.FoodId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn");
                    }

                    foodName = food.Name;
                    newCalories = food.Calories * dto.Quantity;
                }
                else if (mealItem.DishId.HasValue)
                {
                    var dish = await _unitOfWork.PredefinedDishes.GetByIdAsync(mealItem.DishId.Value);
                    if (dish == null)
                    {
                        _logger.LogWarning("Dish with ID {DishId} not found", mealItem.DishId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn định sẵn");
                    }

                    foodName = dish.Name;
                    newCalories = dish.Calories * dto.Quantity;
                }
                else if (mealItem.UserCapturedFoodId.HasValue)
                {
                    var userCapturedFood = await _unitOfWork.UserCapturedFoods.GetByIdAsync(mealItem.UserCapturedFoodId.Value);
                    if (userCapturedFood == null)
                    {
                        _logger.LogWarning("UserCapturedFood with ID {UserCapturedFoodId} not found", mealItem.UserCapturedFoodId.Value);
                        throw new KeyNotFoundException("Không tìm thấy món ăn đã chụp");
                    }

                    foodName = userCapturedFood.Name;
                    newCalories = userCapturedFood.Calories * dto.Quantity;
                }
                else
                {
                    _logger.LogWarning("No food source found for meal item {ItemId}", itemId);
                    throw new InvalidOperationException("Không tìm thấy thông tin món ăn");
                }

                mealItem.Quantity = dto.Quantity;
                mealItem.Calories = newCalories;

                var result = await _unitOfWork.Save();

                if (result)
                {
                    _logger.LogInformation("Meal item updated successfully: {FoodName} (ID: {ItemId}) - Quantity: {OldQuantity} -> {NewQuantity}, Calories: {OldCalories} -> {NewCalories}", 
                        foodName, itemId, oldQuantity, dto.Quantity, oldCalories, newCalories);
                }

                return new UpdateMealItemResponseDTO
                {
                    ItemId = itemId,
                    MealLogId = mealItem.LogId ?? 0,
                    FoodName = foodName,
                    OldQuantity = oldQuantity,
                    NewQuantity = dto.Quantity,
                    OldCalories = oldCalories,
                    NewCalories = newCalories
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
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating meal item {ItemId}", itemId);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }
    }
}
