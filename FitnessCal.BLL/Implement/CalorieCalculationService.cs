using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserHealthDTO.Request;
using FitnessCal.BLL.DTO.UserHealthDTO.Response;
using FitnessCal.DAL.Define;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement;

public class CalorieCalculationService : ICalorieCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CalorieCalculationService> _logger;

    public CalorieCalculationService(IUnitOfWork unitOfWork, ILogger<CalorieCalculationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CalculateCaloriesResponseDTO> CalculateDailyCaloriesAsync(CalculateCaloriesRequestDTO request, Guid userId)
    {
        try
        {
            _logger.LogInformation("Calculating daily calories for user {UserId} with height: {Height}, weight: {Weight}, goal: {Goal}", 
                userId, request.HeightCm, request.WeightKg, request.Goal);

            var bmr = CalculateBMR(request.Gender, request.DateOfBirth, request.HeightCm, request.WeightKg);
            var tdee = CalculateTDEE(bmr, request.ActivityLevel);
            var dailyCalories = AdjustCaloriesForGoal(tdee, request.Goal);

            var explanation = GenerateExplanation(request.Gender, request.Goal, request.ActivityLevel, request.IntensityLevel);
            var recommendation = GenerateRecommendation(request.Goal, dailyCalories, request.IntensityLevel);
            var dietRecommendation = GenerateDietRecommendation(request.DietType, request.Goal, dailyCalories);
            
            var estimatedGoalDate = CalculateEstimatedGoalDate(request.WeightKg, request.TargetWeightKg, request.Goal, request.IntensityLevel);
            var estimatedWeeks = CalculateEstimatedWeeks(request.WeightKg, request.TargetWeightKg, request.Goal, request.IntensityLevel);
            var goalNote = GenerateGoalNote(request.Goal, request.IntensityLevel, estimatedGoalDate);

            var result = new CalculateCaloriesResponseDTO
            {
                BMR = Math.Round(bmr, 0),
                TDEE = Math.Round(tdee, 0),
                DailyCalories = Math.Round(dailyCalories, 0),
                Explanation = explanation,
                Recommendation = recommendation,
                DietRecommendation = dietRecommendation,
                EstimatedGoalDate = estimatedGoalDate,
                EstimatedWeeks = estimatedWeeks,
                GoalNote = goalNote
            };

            await SaveCalorieCalculationToDatabase(userId, result, request);

            _logger.LogInformation("Calories calculated successfully for user {UserId}. BMR: {BMR}, TDEE: {TDEE}, Daily: {Daily}, Weeks: {Weeks}", 
                userId, result.BMR, result.TDEE, result.DailyCalories, result.EstimatedWeeks);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating daily calories for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CalculateCaloriesResponseDTO> CalculateDailyCaloriesForUserAsync(Guid userId)
    {
        try
        {
            var userHealth = await _unitOfWork.UserHealths.GetByIdAsync(userId);
            if (userHealth == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin sức khỏe của người dùng");
            }

            if (!userHealth.DateOfBirth.HasValue || !userHealth.HeightCm.HasValue || !userHealth.WeightKg.HasValue)
            {
                throw new InvalidOperationException("Thiếu thông tin cần thiết để tính toán (ngày sinh, chiều cao, cân nặng)");
            }

            var request = new CalculateCaloriesRequestDTO
            {
                Gender = userHealth.Gender ?? "Male",
                DateOfBirth = userHealth.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue),
                HeightCm = userHealth.HeightCm.Value,
                WeightKg = userHealth.WeightKg.Value,
                TargetWeightKg = userHealth.Goal?.ToLower() == "stay healthy" ? null : userHealth.TargetWeightKg,
                ActivityLevel = userHealth.ActivityLevel ?? "Sedentary",
                Goal = userHealth.Goal ?? "Maintain",
                IntensityLevel = userHealth.Goal?.ToLower() == "stay healthy" ? null : userHealth.IntensityLevel,
                DietType = userHealth.DietType
            };

            return await CalculateDailyCaloriesAsync(request, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating daily calories for user {UserId}", userId);
            throw;
        }
    }

    public double CalculateBMR(string gender, DateTime dateOfBirth, decimal height, decimal weight)
    {
        var age = DateTime.Now.Year - dateOfBirth.Year;
        if (dateOfBirth > DateTime.Now.AddYears(-age)) age--;

        var heightCm = (double)height;
        var weightKg = (double)weight;

        double bmr;
        if (gender.ToLower() == "male" || gender.ToLower() == "nam")
        {
            bmr = 88.362 + (13.397 * weightKg) + (4.799 * heightCm) - (5.677 * age);
        }
        else
        {
            bmr = 447.593 + (9.247 * weightKg) + (3.098 * heightCm) - (4.330 * age);
        }

        return bmr;
    }

    public double CalculateTDEE(double bmr, string activityLevel)
    {
        return activityLevel.ToLower() switch
        {
            "sedentary" or "ít vận động" => bmr * 1.2,
            "lightly active" or "vận động nhẹ" => bmr * 1.375,
            "moderately active" or "vận động vừa" => bmr * 1.55,
            "very active" or "vận động nhiều" => bmr * 1.725,
            "extra active" or "vận động rất nhiều" => bmr * 1.9,
            _ => bmr * 1.2 
        };
    }

    public double AdjustCaloriesForGoal(double tdee, string goal)
    {
        return goal.ToLower() switch
        {
            "lose weight" or "giảm cân" => tdee - 500,
            "gain weight" or "tăng cân" => tdee + 500,
            "maintain" or "duy trì" => tdee,
            "stay healthy" => tdee, 
            _ => tdee 
        };
    }

    private string GenerateExplanation(string gender, string goal, string activityLevel, string? intensityLevel)
    {
        var genderText = gender.ToLower() == "male" || gender.ToLower() == "nam" ? "nam" : "nữ";
        var goalText = goal.ToLower() switch
        {
            "lose weight" or "giảm cân" => "giảm cân",
            "gain weight" or "tăng cân" => "tăng cân",
            "maintain" or "duy trì" => "duy trì cân nặng",
            "stay healthy" => "duy trì sức khỏe",
            _ => "duy trì cân nặng"
        };

        var activityText = activityLevel.ToLower() switch
        {
            "sedentary" or "ít vận động" => "ít vận động",
            "lightly active" or "vận động nhẹ" => "vận động nhẹ",
            "moderately active" or "vận động vừa" => "vận động vừa",
            "very active" or "vận động nhiều" => "vận động nhiều",
            "extra active" or "vận động rất nhiều" => "vận động rất nhiều",
            _ => "ít vận động"
        };

        var intensityText = intensityLevel?.ToLower() switch
        {
            "tối đa" => "tối đa",
            "ổn định" => "ổn định", 
            "từ từ" => "từ từ",
            "thư giãn" => "thư giãn",
            _ => "không xác định"
        };

        var explanationText = goal.ToLower() == "stay healthy" 
            ? $"Dựa trên thông tin của bạn (giới tính: {genderText}, mục tiêu: {goalText}, mức độ hoạt động: {activityText}), " +
              $"chúng tôi đã tính toán lượng calorie cần thiết mỗi ngày."
            : $"Dựa trên thông tin của bạn (giới tính: {genderText}, mục tiêu: {goalText}, mức độ hoạt động: {activityText}, cường độ: {intensityText}), " +
              $"chúng tôi đã tính toán lượng calorie cần thiết mỗi ngày.";

        return explanationText;
    }

    private string GenerateRecommendation(string goal, double dailyCalories, string? intensityLevel)
    {
        var intensityText = GetIntensityText(intensityLevel);
        
        return goal.ToLower() switch
        {
            "lose weight" or "giảm cân" => $"Để giảm cân {intensityText}, hãy tiêu thụ khoảng {dailyCalories:F0} calorie mỗi ngày. " +
                                           "Kết hợp với tập thể dục để đạt kết quả tốt nhất.",
            "gain weight" or "tăng cân" => $"Để tăng cân {intensityText}, hãy tiêu thụ khoảng {dailyCalories:F0} calorie mỗi ngày. " +
                                           "Tập trung vào thực phẩm giàu protein và chất béo tốt.",
            "maintain" or "duy trì" => $"Để duy trì cân nặng hiện tại, hãy tiêu thụ khoảng {dailyCalories:F0} calorie mỗi ngày. " +
                                       "Duy trì chế độ ăn cân bằng và tập thể dục đều đặn.",
            "stay healthy" => $"Để duy trì sức khỏe tốt, hãy tiêu thụ khoảng {dailyCalories:F0} calorie mỗi ngày. " +
                              "Duy trì chế độ ăn cân bằng và tập thể dục đều đặn.",
            _ => $"Lượng calorie khuyến nghị mỗi ngày: {dailyCalories:F0}. " +
                 "Hãy tham khảo ý kiến chuyên gia dinh dưỡng để có kế hoạch phù hợp."
        };
    }

    private string GenerateDietRecommendation(string? dietType, string goal, double dailyCalories)
    {
        if (string.IsNullOrEmpty(dietType))
            return "Hãy chọn chế độ ăn phù hợp với mục tiêu của bạn.";

        return dietType.ToLower() switch
        {
            "eat clean" => "Chế độ Eat Clean: Ưu tiên thực phẩm tự nhiên, ít chế biến. " +
                           "Tập trung vào rau xanh, trái cây, ngũ cốc nguyên hạt và protein nạc.",
            "high protein" => "Chế độ High Protein: Tăng cường protein từ thịt nạc, cá, trứng, đậu. " +
                              "Protein giúp xây dựng cơ bắp và tăng cảm giác no.",
            "low carb" => "Chế độ Low Carb: Giảm tinh bột, tăng chất béo tốt và protein. " +
                          "Phù hợp cho mục tiêu giảm cân nhanh.",
            _ => "Hãy chọn chế độ ăn phù hợp với mục tiêu của bạn."
        };
    }

    private DateTime? CalculateEstimatedGoalDate(decimal currentWeight, decimal? targetWeight, string goal, string? intensityLevel)
    {
        if (!targetWeight.HasValue || currentWeight == targetWeight.Value)
            return null;

        var weightDifference = Math.Abs((double)(currentWeight - targetWeight.Value));
        var weeklyRate = GetWeeklyRate(goal, intensityLevel);
        
        if (weeklyRate <= 0) return null;
        
        var weeks = Math.Ceiling(weightDifference / weeklyRate);
        return DateTime.Now.AddDays(weeks * 7);
    }

    private int CalculateEstimatedWeeks(decimal currentWeight, decimal? targetWeight, string goal, string? intensityLevel)
    {
        if (!targetWeight.HasValue || currentWeight == targetWeight.Value)
            return 0;

        var weightDifference = Math.Abs((double)(currentWeight - targetWeight.Value));
        var weeklyRate = GetWeeklyRate(goal, intensityLevel);
        
        if (weeklyRate <= 0) return 0;
        
        return (int)Math.Ceiling(weightDifference / weeklyRate);
    }

    private double GetWeeklyRate(string goal, string? intensityLevel)
    {
        return (goal.ToLower(), intensityLevel?.ToLower()) switch
        {
            ("lose weight" or "giảm cân", "tối đa") => 1.0,
            ("lose weight" or "giảm cân", "ổn định") => 0.75,
            ("lose weight" or "giảm cân", "từ từ") => 0.5,
            ("lose weight" or "giảm cân", "thư giãn") => 0.25,
            ("gain weight" or "tăng cân", "tối đa") => 0.75,
            ("gain weight" or "tăng cân", "ổn định") => 0.5,
            ("gain weight" or "tăng cân", "từ từ") => 0.25,
            ("gain weight" or "tăng cân", "thư giãn") => 0.15,
            _ => 0.5 
        };
    }

    private string GetIntensityText(string? intensityLevel)
    {
        return intensityLevel?.ToLower() switch
        {
            "tối đa" => "nhanh chóng",
            "ổn định" => "ổn định",
            "từ từ" => "từ từ",
            "thư giãn" => "thoải mái",
            _ => "ổn định"
        };
    }

    private string GenerateGoalNote(string goal, string? intensityLevel, DateTime? estimatedGoalDate)
    {
        if (goal.ToLower() == "stay healthy")
        {
            return "Mục tiêu của bạn là duy trì sức khỏe";
        }

        if (goal.ToLower() == "maintain" || goal.ToLower() == "duy trì")
        {
            return "Mục tiêu của bạn là duy trì cân nặng hiện tại";
        }

        if (estimatedGoalDate.HasValue)
        {
            var dateText = estimatedGoalDate.Value.ToString("dd/MM/yyyy");
            return $"Bạn sẽ đạt mục tiêu vào ngày {dateText}";
        }

        return "Mục tiêu của bạn chưa được xác định";
    }

    private async Task SaveCalorieCalculationToDatabase(Guid userId, CalculateCaloriesResponseDTO result, CalculateCaloriesRequestDTO request)
    {
        try
        {
            var userHealth = await _unitOfWork.UserHealths.GetByIdAsync(userId);
            
            if (userHealth != null)
            {
                userHealth.Gender = request.Gender;
                userHealth.DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth);
                userHealth.HeightCm = request.HeightCm;
                userHealth.WeightKg = request.WeightKg;
                userHealth.TargetWeightKg = request.TargetWeightKg;
                userHealth.ActivityLevel = request.ActivityLevel;
                userHealth.Goal = request.Goal;
                userHealth.IntensityLevel = request.IntensityLevel;
                userHealth.DietType = request.DietType;
                userHealth.DailyCalories = result.DailyCalories;
                userHealth.EstimatedGoalDate = result.EstimatedGoalDate.HasValue 
                    ? DateOnly.FromDateTime(result.EstimatedGoalDate.Value) 
                    : null;
                userHealth.GoalNote = result.GoalNote;
                
                await _unitOfWork.Save();
                
                _logger.LogInformation("Updated UserHealth for user {UserId} with daily calories: {Calories}", 
                    userId, result.DailyCalories);
            }
            else
            {
                _logger.LogInformation("Creating new UserHealth record for user {UserId}", userId);
                
                var newUserHealth = new FitnessCal.Domain.UserHealth
                {
                    UserId = userId,
                    Gender = request.Gender,
                    DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth),
                    HeightCm = request.HeightCm,
                    WeightKg = request.WeightKg,
                    TargetWeightKg = request.TargetWeightKg,
                    ActivityLevel = request.ActivityLevel,
                    Goal = request.Goal,
                    IntensityLevel = request.IntensityLevel,
                    DietType = request.DietType,
                    DailyCalories = result.DailyCalories,
                    EstimatedGoalDate = result.EstimatedGoalDate.HasValue 
                        ? DateOnly.FromDateTime(result.EstimatedGoalDate.Value) 
                        : null,
                    GoalNote = result.GoalNote
                };
                
                await _unitOfWork.UserHealths.AddAsync(newUserHealth);
                await _unitOfWork.Save();
                
                _logger.LogInformation("Created new UserHealth for user {UserId} with daily calories: {Calories}", 
                    userId, result.DailyCalories);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save calorie calculation to database for user {UserId}", userId);
        }
    }
}
