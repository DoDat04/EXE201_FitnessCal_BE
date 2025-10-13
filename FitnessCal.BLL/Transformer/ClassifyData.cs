using FitnessCal.BLL.Define;

namespace FitnessCal.BLL.Tools;

public class ClassifyData
{
    private readonly IGeminiService _geminiService;
    public ClassifyData(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }
    protected internal async Task<string> ClassifyFoodOrDishAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Food";

        var lowerName = name.ToLower().Trim();

        // Một số keyword giúp phân loại nhanh (dùng cho tên đơn giản/ngắn)
        string[] ingredientKeywords = { "trứng", "bắp", "gạo", "thịt", "cá", "rau", "tôm", "muối", "dầu", "nước mắm" };

        // Nếu chỉ có 1 từ, hoặc độ dài < 2 từ => khả năng cao là Food
        if (ingredientKeywords.Any(kw => lowerName == kw))
            return "Food";

        // Nếu chứa các món ăn phổ biến => Dish
        if (lowerName.Contains("cơm") ||
            lowerName.Contains("phở") ||
            lowerName.Contains("salad") ||
            lowerName.Contains("pizza") ||
            lowerName.Contains("bún") ||
            lowerName.Contains("mì"))
        {
            return "Dish";
        }

        // Nếu tên có nhiều hơn 1 từ (ví dụ: "thịt kho tàu", "cá chiên", "trứng luộc") => Dish
        if (lowerName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
            return "Dish";

        // fallback: gọi AI để classify
        var prompt = $@"
        Cho biết '{name}' là nguyên liệu (Food) hay món ăn (Dish).
        Trả lời duy nhất: 'Food' hoặc 'Dish'.";

        var aiResult = await _geminiService.GenerateFoodsAsync(prompt);
        return aiResult.Trim().Equals("Food", StringComparison.OrdinalIgnoreCase) ? "Food" : "Dish";
    }
}