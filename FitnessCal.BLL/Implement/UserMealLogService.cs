using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealLogDTO.Request;
using FitnessCal.BLL.DTO.UserMealLogDTO.Response;
using FitnessCal.BLL.Constants;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class UserMealLogService : IUserMealLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserMealLogService> _logger;

        public UserMealLogService(IUnitOfWork unitOfWork, ILogger<UserMealLogService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CreateUserMealLogResponseDTO> AutoCreateMealLogsAsync(Guid userId, CreateUserMealLogDTO dto)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid UserId provided for meal log creation");
                    throw new ArgumentException(UserMealLogMessage.INVALID_MEAL_DATE);
                }

                if (dto.MealDate == default)
                {
                    _logger.LogWarning("Invalid meal date provided for meal log creation");
                    throw new ArgumentException(UserMealLogMessage.INVALID_MEAL_DATE);
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for meal log creation", userId);
                    throw new KeyNotFoundException(UserMessage.USER_NOT_FOUND);
                }

                var mealTypes = new[] { "Breakfast", "Lunch", "Dinner", "Morning Snack", "Afternoon Snack", "Dinner Snack" };
                var mealLogIds = new List<int>();
                var createdCount = 0;
                var existingCount = 0;

                foreach (var mealType in mealTypes)
                {
                    var existingMealLog = await _unitOfWork.UserMealLogs.GetAllAsync(log => 
                        log.UserId == userId && 
                        log.MealDate == dto.MealDate && 
                        log.MealType == mealType);

                    if (existingMealLog.Any())
                    {
                        var existingLog = existingMealLog.First();
                        mealLogIds.Add(existingLog.LogId);
                        existingCount++;

                        _logger.LogInformation("Meal log already exists for user {UserId} on {MealDate} with type {MealType} (LogId: {LogId})", 
                            userId, dto.MealDate, mealType, existingLog.LogId);
                    }
                    else
                    {
                        var log = new UserMealLog
                        {
                            UserId = userId,
                            MealDate = dto.MealDate,
                            MealType = mealType
                        };

                        await _unitOfWork.UserMealLogs.AddAsync(log);
                        createdCount++;

                        _logger.LogInformation("Meal log created for user {UserId} on {MealDate} with type {MealType}", 
                            userId, dto.MealDate, mealType);
                    }
                }

                // Chỉ save nếu có meal log mới được tạo
                if (createdCount > 0)
                {
                    var result = await _unitOfWork.Save();
                    if (!result)
                    {
                        _logger.LogError("Failed to save meal logs to database for user {UserId} on {MealDate}", userId, dto.MealDate);
                        throw new Exception("Failed to save meal logs to database");
                    }

                    // Lấy lại LogId của các meal log vừa tạo
                    var newlyCreatedLogs = await _unitOfWork.UserMealLogs.GetAllAsync(log => 
                        log.UserId == userId && 
                        log.MealDate == dto.MealDate && 
                        !mealLogIds.Contains(log.LogId));

                    foreach (var newLog in newlyCreatedLogs)
                    {
                        mealLogIds.Add(newLog.LogId);
                    }

                    _logger.LogInformation("Successfully saved {CreatedCount} new meal logs to database for user {UserId} on {MealDate}", 
                        createdCount, userId, dto.MealDate);
                }

                _logger.LogInformation("Auto meal logs completed for user {UserId} on {MealDate}. Created: {CreatedCount}, Existing: {ExistingCount}, Total: {TotalCount}", 
                    userId, dto.MealDate, createdCount, existingCount, mealLogIds.Count);

                return new CreateUserMealLogResponseDTO
                {
                    UserId = userId,
                    MealDate = dto.MealDate,
                    MealLogIds = mealLogIds,
                    Message = UserMealLogMessage.MEAL_LOG_CREATED_SUCCESS
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
                _logger.LogError(ex, "Error occurred while creating meal logs for user {UserId} on {MealDate}", 
                    userId, dto.MealDate);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<GetMealLogsByDateResponseDTO> GetMealLogsByDateAsync(Guid userId, DateOnly date)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    throw new KeyNotFoundException(UserMessage.USER_NOT_FOUND);
                }

                var mealLogs = await _unitOfWork.UserMealLogs.GetAllAsync(log => 
                    log.UserId == userId && log.MealDate == date,
                    log => log.UserMealItems);

                var mealLogSummaries = new List<MealLogSummaryDTO>();

                // Batch load all Food and Dish IDs needed
                var foodIds = mealLogs.SelectMany(log => log.UserMealItems ?? new List<UserMealItem>())
                    .Where(item => item.FoodId.HasValue)
                    .Select(item => item.FoodId.Value)
                    .Distinct()
                    .ToList();

                var dishIds = mealLogs.SelectMany(log => log.UserMealItems ?? new List<UserMealItem>())
                    .Where(item => item.DishId.HasValue)
                    .Select(item => item.DishId.Value)
                    .Distinct()
                    .ToList();

                // Load all foods and dishes in batch
                var foods = new List<Food>();
                var dishes = new List<PredefinedDish>();

                if (foodIds.Any())
                {
                    foods = (await _unitOfWork.Foods.GetAllAsync(f => foodIds.Contains(f.FoodId))).ToList();
                }

                if (dishIds.Any())
                {
                    dishes = (await _unitOfWork.PredefinedDishes.GetAllAsync(d => dishIds.Contains(d.DishId))).ToList();
                }

                // Create dictionaries for fast lookup
                var foodDict = foods.ToDictionary(f => f.FoodId);
                var dishDict = dishes.ToDictionary(d => d.DishId);

                foreach (var mealLog in mealLogs)
                {
                    var totalCalories = mealLog.UserMealItems?.Sum(item => item.Calories ?? 0) ?? 0;
                    var targetCalories = GetTargetCaloriesForMealType(mealLog.MealType ?? "");

                    var items = new List<MealItemDTO>();
                    if (mealLog.UserMealItems != null)
                    {
                        foreach (var item in mealLog.UserMealItems)
                        {
                            string foodName = "Unknown";
                            
                            // Fast lookup from dictionaries
                            if (item.FoodId.HasValue && foodDict.TryGetValue(item.FoodId.Value, out var food))
                            {
                                foodName = food.Name;
                            }
                            else if (item.DishId.HasValue && dishDict.TryGetValue(item.DishId.Value, out var dish))
                            {
                                foodName = dish.Name;
                            }

                            items.Add(new MealItemDTO
                            {
                                ItemId = item.ItemId,
                                FoodName = foodName,
                                Quantity = item.Quantity,
                                Calories = item.Calories ?? 0
                            });
                        }
                    }

                    mealLogSummaries.Add(new MealLogSummaryDTO
                    {
                        LogId = mealLog.LogId,
                        MealType = mealLog.MealType ?? "",
                        TotalCalories = totalCalories,
                        TargetCalories = targetCalories,
                        Items = items
                    });
                }

                _logger.LogInformation("Retrieved meal logs for user {UserId} on {Date} with {FoodCount} foods and {DishCount} dishes loaded in batch", 
                    userId, date, foods.Count, dishes.Count);

                return new GetMealLogsByDateResponseDTO
                {
                    UserId = userId,
                    MealDate = date,
                    MealLogs = mealLogSummaries
                };
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving meal logs for user {UserId} on {Date}", userId, date);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        private double GetTargetCaloriesForMealType(string mealType)
        {
            return mealType switch
            {
                "Breakfast" => 600,
                "Lunch" => 800,
                "Dinner" => 600,
                "Morning Snack" => 300,
                "Afternoon Snack" => 300,
                "Dinner Snack" => 200,
                _ => 600
            };
        }
    }
}
