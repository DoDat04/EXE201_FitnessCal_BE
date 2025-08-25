using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.MealPlanningDTO;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

namespace FitnessCal.BLL.Implement
{
    public class MealPlanningService : IMealPlanningService
    {
        private readonly IFoodRepository _foodRepository;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<MealPlanningService> _logger;

        private readonly IUserHealthRepository _userHealthRepository;
        private readonly IUserMealLogRepository _userMealLogRepository;
        private readonly IUserMealItemRepository _userMealItemRepository;

        public MealPlanningService(
            IFoodRepository foodRepository,
            IUserHealthRepository userHealthRepository,
            IGeminiService geminiService,
            IUserMealLogRepository userMealLogRepository,
            IUserMealItemRepository userMealItemRepository,
            ILogger<MealPlanningService> logger)
        {
            _foodRepository = foodRepository;
            _userHealthRepository = userHealthRepository;
            _geminiService = geminiService;
            _userMealLogRepository = userMealLogRepository;
            _userMealItemRepository = userMealItemRepository;
            _logger = logger;
        }

        public async Task<MealPlanningResponseDTO> GenerateMealPlanAsync(Guid userId)
        {
            try
            {
                // 1. Lấy danh sách food có sẵn
                var availableFoods = await _foodRepository.GetAllAsync();
                
                // 2. Lấy user health để biết DailyCalories
                var userHealth = (await _userHealthRepository.GetAllAsync(u => u.UserId == userId)).FirstOrDefault();
                if (userHealth == null || userHealth.DailyCalories == null)
                {
                    throw new InvalidOperationException("User health or daily calories not found");
                }

                var dailyCalories = userHealth.DailyCalories.Value;
                var mealCount = DecideMealCount(dailyCalories);

                // 3. Tạo prompt cho Gemini
                var prompt = GeneratePrompt(availableFoods.ToList(), null, userHealth, mealCount, dailyCalories);
                
                // 4. Gọi Gemini API
                var geminiResponse = await _geminiService.GenerateMealPlanAsync(prompt);
                
                // 5. Parse response thành MealPlan
                var mealPlan = ParseGeminiResponse(geminiResponse, availableFoods.ToList());
                
                // 6. Tính toán và validate
                mealPlan = CalculateAndValidateNutrition(mealPlan, dailyCalories);
                
                // 7. Lưu meal plan vào database
                await SaveMealPlanToDatabase(userId, mealPlan);
                
                // 8. Set response data
                var response = new MealPlanningResponseDTO
                {
                    UserId = userId,
                    GeneratedDate = DateTime.UtcNow,
                    DailyTarget = new NutritionTargetDTO
                    {
                        TotalCalories = dailyCalories,
                        TotalProtein = CalculateTargetProtein(dailyCalories),
                        TotalCarbs = CalculateTargetCarbs(dailyCalories),
                        TotalFat = CalculateTargetFat(dailyCalories)
                    },
                    ActualDaily = CalculateDailyNutrition(mealPlan.Meals),
                    Meals = mealPlan.Meals
                };

                _logger.LogInformation("Meal plan generated successfully for user {UserId}", userId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating meal plan for user {UserId}", userId);
                throw;
            }
        }

        private int DecideMealCount(double dailyCalories)
        {
            if (dailyCalories >= 3000) return 5; // 5–6 bữa
            if (dailyCalories >= 2500) return 4; // 3 chính + 1–2 phụ
            if (dailyCalories >= 2000) return 4; // 3 chính + 1 phụ
            return 3; // <= 2000: 3 bữa
        }

        private string GeneratePrompt(List<Food> foods, MealPlanningRequestDTO? _unused, UserHealth? userHealth, int mealCount, double dailyCalories)
        {
            var foodList = string.Join("\n", foods.Select(f => 
                $"- [{f.FoodId}] {f.Name}: {f.Calories} cal, {f.Protein}g protein, {f.Carbs}g carbs, {f.Fat}g fat"));

            var userContext = "";
            if (userHealth != null)
            {
                userContext = $@"
Thông tin người dùng:
- Chiều cao: {userHealth.HeightCm}cm
- Cân nặng: {userHealth.WeightKg}kg
- Mục tiêu: {userHealth.Goal}
- Mức độ hoạt động: {userHealth.ActivityLevel}
- Loại chế độ ăn: {userHealth.DietType}";
            }

            var excludedFoods = "";

            return $@"
Bạn là chuyên gia dinh dưỡng người Việt Nam. Hãy tạo thực đơn {mealCount} bữa với tổng calories gần nhất {dailyCalories} cal.

{userContext}

Danh sách thực phẩm có sẵn:
{foodList}

Yêu cầu:
1. Chia thành đúng {mealCount} bữa từ các bữa sau (chọn phù hợp với daily_calories):
   - Breakfast (Bữa sáng)
   - Lunch (Bữa trưa) 
   - Dinner (Bữa tối)
   - Morning Snack (Bữa phụ sáng)
   - Afternoon Snack (Bữa phụ chiều)
   - Dinner Snack (Bữa phụ tối)
2. Mỗi bữa phải có từ 2 đến 4 món ăn (không để trống)
3. Tổng calories phải gần nhất với {dailyCalories}
4. Cân bằng protein, carbs, fat theo tỷ lệ chuẩn:
   - Protein: 20-25% tổng calories (khoảng {dailyCalories * 0.22 / 4:F0}g)
   - Carbs: 45-55% tổng calories (khoảng {dailyCalories * 0.5 / 4:F0}g)  
   - Fat: 20-35% tổng calories (khoảng {dailyCalories * 0.28 / 4:F0}g)
5. KHÔNG để protein quá cao (>25% tổng calories) - ưu tiên carbs và fat
6. Mỗi bữa phải có đủ 3 nhóm: tinh bột (gạo, khoai, bánh), đạm (thịt, cá, đậu), rau củ
7. Ưu tiên món ăn Việt Nam nếu có thể
8. Chọn món ăn phù hợp với từng bữa
9. Chỉ trả về JSON hợp lệ, không kèm bất kỳ văn bản, markdown hay ``` nào.
10. Quantity là bội số của 100g (ví dụ 1 = 100g, 1.5 = 150g). Dữ liệu dinh dưỡng trong bảng Food là trên 100g.
11. Tổng calories cả ngày trong khoảng ±5% so với {dailyCalories}; mỗi bữa trong khoảng ±10% so với mục tiêu bữa.
12. BẮT BUỘC chọn món từ danh sách trên và điền đúng foodId tương ứng. Không tự tạo tên/ID mới.
13. BẮT BUỘC sử dụng đúng tên bữa ăn như đã liệt kê ở trên (Breakfast, Lunch, Dinner, Morning Snack, Afternoon Snack, Dinner Snack).

Trả về JSON với format chính xác (quantity là bội số 100g):
{{
  ""meals"": [
    {{
      ""mealType"": ""Breakfast"",
      ""mealName"": ""Bữa sáng"",
      ""foods"": [
        {{
          ""foodId"": 1,
          ""foodName"": ""Tên món"",
          ""quantity"": 1.0
        }}
      ]
    }}
  ]
}}
";
        }

        private MealPlanningResponseDTO ParseGeminiResponse(string geminiResponse, List<Food> availableFoods)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(geminiResponse))
                {
                    throw new InvalidOperationException("AI trả về rỗng");
                }

                _logger.LogDebug("Gemini raw response: {Resp}", geminiResponse);

                // Loại bỏ code fences/backticks/markdown
                var cleaned = Regex.Replace(geminiResponse, "^```[a-zA-Z0-9]*\\s*|```$", string.Empty, RegexOptions.Multiline).Trim();
                // Xoá tất cả dấu ``` nếu còn vương
                cleaned = cleaned.Replace("```", string.Empty);

                // Tìm đoạn JSON đầu tiên (object hoặc array)
                string jsonContent = ExtractFirstJson(cleaned);
                if (string.IsNullOrEmpty(jsonContent))
                {
                    _logger.LogWarning("Không trích xuất được JSON từ phản hồi AI");
                    throw new InvalidOperationException("Không tìm thấy JSON hợp lệ trong phản hồi AI");
                }

                var trimmed = jsonContent.TrimStart();
                if (trimmed.StartsWith("["))
                {
                    // AI trả về mảng ở root → bọc vào object { "meals": [...] }
                    jsonContent = $"{{\"meals\": {jsonContent} }}";
                }

                _logger.LogDebug("Gemini extracted JSON (first 500 chars): {Chunk}", jsonContent.Length > 500 ? jsonContent.Substring(0, 500) : jsonContent);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                var geminiMealPlan = JsonSerializer.Deserialize<GeminiMealPlanResponse>(jsonContent, options);

                if (geminiMealPlan?.Meals == null || geminiMealPlan.Meals.Count == 0)
                {
                    _logger.LogWarning("JSON parse OK nhưng không có meals");
                    throw new InvalidOperationException("AI không tạo được bữa ăn. Vui lòng thử lại.");
                }

                var meals = geminiMealPlan.Meals.Select(gm => new MealDTO
                {
                    MealType = gm.MealType,
                    MealName = gm.MealName,
                    TargetCalories = 0,
                    ActualCalories = 0,
                    Foods = gm.Foods.Select(gf =>
                    {
                        // Resolve id by id first, then by name fallback
                        int resolvedId = gf.FoodId;
                        var baseNutri = GetFoodNutrition(gf.FoodId, gf.FoodName, availableFoods, out resolvedId);
                        return new MealFoodDTO
                        {
                            FoodId = resolvedId,
                            FoodName = gf.FoodName,
                            Quantity = gf.Quantity,
                            BaseNutrition = baseNutri,
                            CalculatedNutrition = new NutritionInfoDTO()
                        };
                    }).ToList(),
                }).ToList();

                return new MealPlanningResponseDTO
                {
                    Meals = meals
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ParseGeminiResponse failed");
                throw new InvalidOperationException("Failed to parse meal plan from AI response", ex);
            }
        }

        private static string ExtractFirstJson(string text)
        {
            // Quét tìm cặp ngoặc {} hoặc [] cân bằng ở mức top-level
            int objLevel = 0;
            int arrLevel = 0;
            int start = -1;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '{')
                {
                    if (objLevel == 0 && arrLevel == 0)
                    {
                        start = i;
                    }
                    objLevel++;
                }
                else if (c == '}')
                {
                    objLevel--;
                    if (objLevel == 0 && arrLevel == 0 && start >= 0)
                    {
                        return text.Substring(start, i - start + 1);
                    }
                }
                else if (c == '[')
                {
                    if (objLevel == 0 && arrLevel == 0)
                    {
                        start = i;
                    }
                    arrLevel++;
                }
                else if (c == ']')
                {
                    arrLevel--;
                    if (objLevel == 0 && arrLevel == 0 && start >= 0)
                    {
                        return text.Substring(start, i - start + 1);
                    }
                }
            }
            return string.Empty;
        }

        private NutritionInfoDTO GetFoodNutrition(int foodId, string? foodName, List<Food> availableFoods, out int resolvedFoodId)
        {
            // Try by id
            var food = availableFoods.FirstOrDefault(f => f.FoodId == foodId);
            if (food == null && !string.IsNullOrWhiteSpace(foodName))
            {
                // Try exact name (case-insensitive)
                food = availableFoods.FirstOrDefault(f => string.Equals(f.Name, foodName, StringComparison.OrdinalIgnoreCase));
                if (food == null)
                {
                    // Try contains
                    food = availableFoods.FirstOrDefault(f => f.Name.Contains(foodName, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (food == null)
            {
                resolvedFoodId = foodId;
                return new NutritionInfoDTO();
            }

            resolvedFoodId = food.FoodId;
 
            return new NutritionInfoDTO
            {
                Calories = food.Calories,
                Protein = food.Protein,
                Carbs = food.Carbs,
                Fat = food.Fat
            };
        }

        private MealPlanningResponseDTO CalculateAndValidateNutrition(MealPlanningResponseDTO mealPlan, double targetCalories)
        {
            var mealCount = mealPlan.Meals.Count;
            var targetCaloriesPerMeal = targetCalories / mealCount;
            var mealTolerance = 0.10; // ±10%
            var dailyTolerance = 100.0; // thiếu tối đa 100 calo

            // Tính toán target macro nutrients cho cả ngày
            var targetProtein = CalculateTargetProtein(targetCalories);
            var targetCarbs = CalculateTargetCarbs(targetCalories);
            var targetFat = CalculateTargetFat(targetCalories);

            // Tính toán target macro nutrients cho mỗi bữa
            var targetProteinPerMeal = targetProtein / mealCount;
            var targetCarbsPerMeal = targetCarbs / mealCount;
            var targetFatPerMeal = targetFat / mealCount;

            foreach (var meal in mealPlan.Meals)
            {
                meal.TargetCalories = targetCaloriesPerMeal;

                // Tính nutrition cho từng food lần đầu
                foreach (var food in meal.Foods)
                {
                    var factor = food.Quantity; // bội số 100g
                    food.CalculatedNutrition = new NutritionInfoDTO
                    {
                        Calories = RoundToDecimal(food.BaseNutrition.Calories * factor, 1),
                        Protein = RoundToDecimal(food.BaseNutrition.Protein * factor, 1),
                        Carbs = RoundToDecimal(food.BaseNutrition.Carbs * factor, 1),
                        Fat = RoundToDecimal(food.BaseNutrition.Fat * factor, 1)
                    };
                }

                // Tính tổng cho bữa ăn
                meal.ActualCalories = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Calories), 1);

                // Nếu lệch quá nhiều so với mục tiêu, scale quantity theo tỷ lệ
                if (meal.ActualCalories > 0)
                {
                    var lower = RoundToDecimal(meal.TargetCalories * (1 - mealTolerance), 1);
                    var upper = RoundToDecimal(meal.TargetCalories * (1 + mealTolerance), 1);
                    if (meal.ActualCalories < lower || meal.ActualCalories > upper)
                    {
                        var scale = RoundToDecimal(meal.TargetCalories / meal.ActualCalories, 1);
                        foreach (var food in meal.Foods)
                        {
                            food.Quantity *= scale;
                            // làm tròn quantity theo 0.1 và kẹp min/max hợp lý
                            food.Quantity = Math.Round(food.Quantity * 10, MidpointRounding.AwayFromZero) / 10.0;
                            if (food.Quantity < 0.3) food.Quantity = 0.3; // tối thiểu ~30g
                            if (food.Quantity > 3.0) food.Quantity = 3.0;   // tối đa ~300g

                            var factor = food.Quantity; // bội số 100g
                            food.CalculatedNutrition = new NutritionInfoDTO
                            {
                                Calories = RoundToDecimal(food.BaseNutrition.Calories * factor, 1),
                                Protein = RoundToDecimal(food.BaseNutrition.Protein * factor, 1),
                                Carbs = RoundToDecimal(food.BaseNutrition.Carbs * factor, 1),
                                Fat = RoundToDecimal(food.BaseNutrition.Fat * factor, 1)
                            };
                        }
                        meal.ActualCalories = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Calories), 1);
                    }
                }

                // Cập nhật tổng dinh dưỡng bữa
                meal.MealNutrition = new NutritionInfoDTO
                {
                    Calories = RoundToDecimal(meal.ActualCalories, 1),
                    Protein = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Protein), 1),
                    Carbs = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Carbs), 1),
                    Fat = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Fat), 1)
                };
            }

            // Validation cuối cùng: đảm bảo actualDaily không vượt dailyTarget và chỉ thiếu tối đa 100 calo
            var actualDaily = CalculateDailyNutrition(mealPlan.Meals);
            
            // Kiểm tra macro nutrients balance
            var proteinRatio = actualDaily.TotalProtein / targetProtein;
            var carbsRatio = actualDaily.TotalCarbs / targetCarbs;
            var fatRatio = actualDaily.TotalFat / targetFat;
            
            // Nếu macro nutrients mất cân bằng quá nhiều, điều chỉnh
            var maxRatio = Math.Max(Math.Max(proteinRatio, carbsRatio), fatRatio);
            var minRatio = Math.Min(Math.Min(proteinRatio, carbsRatio), fatRatio);
            
            if (maxRatio > 1.3 || minRatio < 0.8) // Giảm ngưỡng để điều chỉnh sớm hơn
            {
                _logger.LogInformation("Macro nutrients imbalance detected. Protein: {ProteinRatio:F2}, Carbs: {CarbsRatio:F2}, Fat: {FatRatio:F2}. Adjusting...", 
                    proteinRatio, carbsRatio, fatRatio);
                
                // Điều chỉnh quantity để cân bằng macro nutrients
                var adjustmentFactor = 1.0;
                
                if (proteinRatio > 1.2) // Protein quá cao (>120%)
                {
                    adjustmentFactor = 0.75; // Giảm 25% để giảm protein mạnh hơn nữa
                    _logger.LogInformation("Protein too high ({ProteinRatio:F2}), reducing quantities by 25%", proteinRatio);
                }
                else if (proteinRatio > 1.15) // Protein hơi cao (>115%)
                {
                    adjustmentFactor = 0.82; // Giảm 18% để giảm protein
                    _logger.LogInformation("Protein slightly high ({ProteinRatio:F2}), reducing quantities by 18%", proteinRatio);
                }
                else if (proteinRatio > 1.1) // Protein hơi cao (>110%)
                {
                    adjustmentFactor = 0.88; // Giảm 12%
                    _logger.LogInformation("Protein slightly high ({ProteinRatio:F2}), reducing quantities by 12%", proteinRatio);
                }
                else if (fatRatio > 1.25) // Fat quá cao (>125%)
                {
                    adjustmentFactor = 0.75; // Giảm 25% để giảm fat mạnh hơn
                    _logger.LogInformation("Fat very high ({FatRatio:F2}), reducing quantities by 25%", fatRatio);
                }
                else if (fatRatio > 1.3) // Fat quá cao (>130%)
                {
                    adjustmentFactor = 0.80; // Giảm 20% để giảm fat
                    _logger.LogInformation("Fat too high ({FatRatio:F2}), reducing quantities by 20%", fatRatio);
                }
                else if (fatRatio > 1.2) // Fat hơi cao (>120%)
                {
                    adjustmentFactor = 0.88; // Giảm 12% để giảm fat
                    _logger.LogInformation("Fat slightly high ({FatRatio:F2}), reducing quantities by 12%", fatRatio);
                }
                else if (carbsRatio < 0.75) // Carbs rất thấp (<75%)
                {
                    adjustmentFactor = 1.40; // Tăng 40% để tăng carbs mạnh nhất
                    _logger.LogInformation("Carbs extremely low ({CarbsRatio:F2}), increasing quantities by 40%", carbsRatio);
                }
                else if (carbsRatio < 0.8) // Carbs rất thấp (<80%)
                {
                    adjustmentFactor = 1.30; // Tăng 30% để tăng carbs mạnh hơn nữa
                    _logger.LogInformation("Carbs very low ({CarbsRatio:F2}), increasing quantities by 30%", carbsRatio);
                }
                else if (carbsRatio < 0.85) // Carbs quá thấp (<85%)
                {
                    adjustmentFactor = 1.20; // Tăng 20% để tăng carbs mạnh hơn
                    _logger.LogInformation("Carbs too low ({CarbsRatio:F2}), increasing quantities by 20%", carbsRatio);
                }
                else if (carbsRatio < 0.9) // Carbs quá thấp (<90%)
                {
                    adjustmentFactor = 1.15; // Tăng 15% để tăng carbs
                    _logger.LogInformation("Carbs too low ({CarbsRatio:F2}), increasing quantities by 15%", carbsRatio);
                }
                else if (carbsRatio < 0.95) // Carbs hơi thấp (<95%)
                {
                    adjustmentFactor = 1.08; // Tăng 8%
                    _logger.LogInformation("Carbs slightly low ({CarbsRatio:F2}), increasing quantities by 8%", carbsRatio);
                }
                else if (fatRatio < 0.65) // Fat quá thấp (<65%)
                {
                    adjustmentFactor = 1.25; // Tăng 25% để tăng fat mạnh hơn
                    _logger.LogInformation("Fat very low ({FatRatio:F2}), increasing quantities by 25%", fatRatio);
                }
                else if (fatRatio < 0.7) // Fat quá thấp (<70%)
                {
                    adjustmentFactor = 1.20; // Tăng 20% để tăng fat
                    _logger.LogInformation("Fat too low ({FatRatio:F2}), increasing quantities by 20%", fatRatio);
                }
                else if (fatRatio < 0.85) // Fat hơi thấp (<85%)
                {
                    adjustmentFactor = 1.12; // Tăng 12%
                    _logger.LogInformation("Fat slightly low ({FatRatio:F2}), increasing quantities by 12%", fatRatio);
                }
                
                if (adjustmentFactor != 1.0)
                {
                    foreach (var meal in mealPlan.Meals)
                    {
                        foreach (var food in meal.Foods)
                        {
                            food.Quantity *= adjustmentFactor;
                            food.Quantity = Math.Round(food.Quantity * 10, MidpointRounding.AwayFromZero) / 10.0;
                            if (food.Quantity < 0.3) food.Quantity = 0.3;
                            if (food.Quantity > 3.0) food.Quantity = 3.0;

                            var factor = food.Quantity;
                            food.CalculatedNutrition = new NutritionInfoDTO
                            {
                                Calories = RoundToDecimal(food.BaseNutrition.Calories * factor, 1),
                                Protein = RoundToDecimal(food.BaseNutrition.Protein * factor, 1),
                                Carbs = RoundToDecimal(food.BaseNutrition.Carbs * factor, 1),
                                Fat = RoundToDecimal(food.BaseNutrition.Fat * factor, 1)
                            };
                        }
                        meal.ActualCalories = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Calories), 1);
                        meal.MealNutrition = new NutritionInfoDTO
                        {
                            Calories = RoundToDecimal(meal.ActualCalories, 1),
                            Protein = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Protein), 1),
                            Carbs = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Carbs), 1),
                            Fat = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Fat), 1)
                        };
                    }
                    
                    // Recalculate daily nutrition
                    actualDaily = CalculateDailyNutrition(mealPlan.Meals);
                    
                    // Log kết quả sau điều chỉnh
                    var newProteinRatio = actualDaily.TotalProtein / targetProtein;
                    var newCarbsRatio = actualDaily.TotalCarbs / targetCarbs;
                    var newFatRatio = actualDaily.TotalFat / targetFat;
                    _logger.LogInformation("After adjustment - Protein: {NewProteinRatio:F2}, Carbs: {NewCarbsRatio:F2}, Fat: {NewFatRatio:F2}", 
                        newProteinRatio, newCarbsRatio, newFatRatio);
                }
            }
            
            if (actualDaily.TotalCalories > targetCalories)
            {
                // Scale down toàn bộ để actualDaily = targetCalories
                var scaleDown = RoundToDecimal(targetCalories / actualDaily.TotalCalories, 1);
                foreach (var meal in mealPlan.Meals)
                {
                    foreach (var food in meal.Foods)
                    {
                        food.Quantity *= scaleDown;
                        food.Quantity = Math.Round(food.Quantity * 10, MidpointRounding.AwayFromZero) / 10.0;
                        if (food.Quantity < 0.3) food.Quantity = 0.3;

                        var factor = food.Quantity;
                        food.CalculatedNutrition = new NutritionInfoDTO
                        {
                            Calories = RoundToDecimal(food.BaseNutrition.Calories * factor, 1),
                            Protein = RoundToDecimal(food.BaseNutrition.Protein * factor, 1),
                            Carbs = RoundToDecimal(food.BaseNutrition.Carbs * factor, 1),
                            Fat = RoundToDecimal(food.BaseNutrition.Fat * factor, 1)
                        };
                    }
                    meal.ActualCalories = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Calories), 1);
                    meal.MealNutrition = new NutritionInfoDTO
                    {
                        Calories = RoundToDecimal(meal.ActualCalories, 1),
                        Protein = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Protein), 1),
                        Carbs = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Carbs), 1),
                        Fat = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Fat), 1)
                    };
                }
            }
            else if (actualDaily.TotalCalories < (targetCalories - dailyTolerance))
            {
                // Scale up nhẹ để đạt tối thiểu (targetCalories - 100)
                var minTarget = RoundToDecimal(targetCalories - dailyTolerance, 1);
                var scaleUp = RoundToDecimal(minTarget / actualDaily.TotalCalories, 1);
                foreach (var meal in mealPlan.Meals)
                {
                    foreach (var food in meal.Foods)
                    {
                        food.Quantity *= scaleUp;
                        food.Quantity = Math.Round(food.Quantity * 10, MidpointRounding.AwayFromZero) / 10.0;
                        if (food.Quantity > 3.0) food.Quantity = 3.0;

                        var factor = food.Quantity;
                        food.CalculatedNutrition = new NutritionInfoDTO
                        {
                            Calories = RoundToDecimal(food.BaseNutrition.Calories * factor, 1),
                            Protein = RoundToDecimal(food.BaseNutrition.Protein * factor, 1),
                            Carbs = RoundToDecimal(food.BaseNutrition.Carbs * factor, 1),
                            Fat = RoundToDecimal(food.BaseNutrition.Fat * factor, 1)
                        };
                    }
                    meal.ActualCalories = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Calories), 1);
                    meal.MealNutrition = new NutritionInfoDTO
                    {
                        Calories = RoundToDecimal(meal.ActualCalories, 1),
                        Protein = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Protein), 1),
                        Carbs = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Carbs), 1),
                        Fat = RoundToDecimal(meal.Foods.Sum(f => f.CalculatedNutrition.Fat), 1)
                    };
                }
            }

            return mealPlan;
        }

        private NutritionActualDTO CalculateDailyNutrition(List<MealDTO> meals)
        {
            return new NutritionActualDTO
            {
                TotalCalories = RoundToDecimal(meals.Sum(m => m.ActualCalories), 1),
                TotalProtein = RoundToDecimal(meals.Sum(m => m.MealNutrition.Protein), 1),
                TotalCarbs = RoundToDecimal(meals.Sum(m => m.MealNutrition.Carbs), 1),
                TotalFat = RoundToDecimal(meals.Sum(m => m.MealNutrition.Fat), 1)
            };
        }

        private double CalculateTargetProtein(double dailyCalories)
        {
            // 15-25% của daily calories, 1g protein = 4 cal
            return (dailyCalories * 0.20) / 4;
        }

        private double CalculateTargetCarbs(double dailyCalories)
        {
            // 45-65% của daily calories, 1g carbs = 4 cal
            return (dailyCalories * 0.55) / 4;
        }

        private double CalculateTargetFat(double dailyCalories)
        {
            // 20-35% của daily calories, 1g fat = 9 cal
            return (dailyCalories * 0.275) / 9;
        }

        private async Task SaveMealPlanToDatabase(Guid userId, MealPlanningResponseDTO mealPlan)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                
                // Lấy tất cả MealLog hiện tại của user cho ngày hôm nay
                var existingMealLogs = (await _userMealLogRepository.GetAllAsync(ml => 
                    ml.UserId == userId && ml.MealDate == today)).ToList();
                
                if (!existingMealLogs.Any())
                {
                    _logger.LogWarning("No meal logs found for user {UserId} on {Date}. Skipping meal items insertion.", userId, today);
                    return;
                }
                
                // Xóa tất cả UserMealItem cũ trước khi insert mới (tránh duplicate)
                var allExistingMealItems = new List<UserMealItem>();
                foreach (var mealLog in existingMealLogs)
                {
                    var existingItems = await _userMealItemRepository.GetAllAsync(item => item.LogId == mealLog.LogId);
                    allExistingMealItems.AddRange(existingItems);
                }
                
                if (allExistingMealItems.Any())
                {
                    foreach (var item in allExistingMealItems)
                    {
                        await _userMealItemRepository.DeleteAsync(item);
                    }
                    _logger.LogInformation("Deleted {Count} existing meal items for user {UserId} on {Date}", 
                        allExistingMealItems.Count, userId, today);
                }
                
                // Insert UserMealItem mới vào đúng MealLog tương ứng với MealType
                foreach (var meal in mealPlan.Meals)
                {
                    // Tìm MealLog tương ứng với MealType
                    var correspondingMealLog = existingMealLogs.FirstOrDefault(ml => 
                        string.Equals(ml.MealType, meal.MealType, StringComparison.OrdinalIgnoreCase));
                    
                    if (correspondingMealLog == null)
                    {
                        _logger.LogWarning("No meal log found for meal type {MealType} of user {UserId}. Skipping this meal.", meal.MealType, userId);
                        continue;
                    }
                    
                    // Insert tất cả food items của bữa này
                    foreach (var food in meal.Foods)
                    {
                        var mealItem = new UserMealItem
                        {
                            LogId = correspondingMealLog.LogId,
                            IsCustom = false,
                            FoodId = food.FoodId,
                            Quantity = food.Quantity,
                            Calories = RoundToDecimal(food.CalculatedNutrition.Calories, 1)
                        };
                        
                        await _userMealItemRepository.AddAsync(mealItem);
                    }
                    
                    _logger.LogInformation("Inserted {FoodCount} food items for meal {MealType} with LogId {LogId}", 
                        meal.Foods.Count, meal.MealType, correspondingMealLog.LogId);
                }
                
                // GenericRepository không tự động save changes, cần gọi SaveChangesAsync
                await _userMealItemRepository.SaveChangesAsync();
                
                _logger.LogInformation("Meal plan items replaced successfully for user {UserId}. Total meals processed: {MealCount}", 
                    userId, mealPlan.Meals.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save meal plan items to database for user {UserId}", userId);
                throw new InvalidOperationException("Failed to save meal plan items to database", ex);
            }
        }

        private static double RoundToDecimal(double value, int decimalPlaces)
        {
            var multiplier = Math.Pow(10, decimalPlaces);
            return Math.Round(value * multiplier, MidpointRounding.AwayFromZero) / multiplier;
        }
    }

    // DTO để parse Gemini response
    public class GeminiMealPlanResponse
    {
        public List<GeminiMeal> Meals { get; set; } = new();
    }

    public class GeminiMeal
    {
        public string MealType { get; set; } = string.Empty;
        public string MealName { get; set; } = string.Empty;
        public List<GeminiFood> Foods { get; set; } = new();
    }

    public class GeminiFood
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public double Quantity { get; set; }
    }
}
