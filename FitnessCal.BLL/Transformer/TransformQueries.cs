using System.Text;
using System.Text.RegularExpressions;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FoodDTO.Response;

namespace FitnessCal.BLL.Transformer;

public class TransformQueries
{
    private readonly IGeminiService _geminiService;

    public TransformQueries(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    protected internal static string TransformQuery(string imageUrl)
    {
        return $@"
        Phân tích ảnh sau: {imageUrl}

        Nhiệm vụ:
        0. Trước tiên, kiểm tra ảnh có phải liên quan đến đồ ăn/món ăn hợp lệ hay không.
           - Nếu KHÔNG phải đồ ăn (ảnh phong cảnh, người, đồ vật, động vật sống...) → trả về duy nhất: INVALID
           - Nếu là món ăn NHẠY CẢM hoặc bị cấm (ví dụ: thịt chó, món liên quan động vật hoang dã...) → trả về duy nhất: INVALID

        1. Nếu hợp lệ: Nhận diện món ăn chính và các thành phần có thể thấy rõ (ví dụ: thịt, bún, rau, nước dùng...).
        2. Xác định loại món ăn (eat clean, món Á, món Âu, fast food, salad, v.v.) để ước lượng phù hợp.
        3. Ước lượng dinh dưỡng dựa trên **khẩu phần trung bình thông thường của người Việt (khoảng 1 tô hoặc 1 đĩa vừa, 700–800g)**.
        4. **Không tính phần nước dư hoặc dầu mỡ nổi trong tô**, chỉ tính phần ăn chính (bún, thịt, chả, rau...).
        5. Đưa ra 4 giá trị chính: Calories (kcal), Carbs (g), Fat (g), Protein (g).
        6. Trả về đúng format duy nhất:
        Tên món|Calories|Carbs|Fat|Protein
        7. Không ghi chú, không mô tả thêm.
        8. Ví dụ:
        Cơm gà xối mỡ|720|65|30|45
        Salad ức gà và rau củ|340|25|9|38
    ";
    }
    
    protected internal async Task<string> TransformUserQueryAsync(string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
            return string.Empty;

        // 1. Chuẩn hóa sơ bộ
        var normalized = userPrompt.Trim().ToLowerInvariant();

        // 2. Stopwords cứng (loại nhanh mấy từ phổ biến)
        var stopwords = new List<string>
        {
            "hãy", "cho", "tôi", "xin", "thông tin", "về", "biết", 
            "cung cấp", "là", "những", "các", "món", "ăn",
            "ờ", "ừ", "ờm", "à", "ừm", "thì", "mà", "nhé", "ạ", 
            "luôn", "nha", "này", "kia", "đi", "với", "coi", "xem", "nhỉ"
        };

        foreach (var sw in stopwords)
        {
            var pattern = $@"\b{Regex.Escape(sw)}\b";
            normalized = Regex.Replace(normalized, pattern, " ", RegexOptions.IgnoreCase);
        }

        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        // 3. Gọi AI để làm sạch thêm stopwords mềm
        var prompt = $@"
            Bạn là một bộ lọc ngôn ngữ. 
            Người dùng có thể nhập câu văn nói khi tìm món ăn. 
            Nhiệm vụ: chỉ giữ lại từ khóa liên quan đến món ăn hoặc nguyên liệu, 
            bỏ toàn bộ từ vô nghĩa, stopwords, filler.

            Ví dụ:
            - 'ờ cho tôi biết món ăn cơm tấm sườn bì chả với' → 'cơm tấm sườn bì chả'
            - 'xin thông tin về ức gà nha' → 'ức gà'
            - 'thì cho tôi coi về món salad healthy luôn' → 'salad healthy'

            Câu đầu vào: '{normalized}'
            Kết quả: ";

        var aiResult = await _geminiService.GenerateFoodsAsync(prompt);

        return aiResult.Trim();
    }
    
    protected internal static string GenerateFoodPrompt(IEnumerable<FoodResponseDTO> foods)
    {
        var sb = new StringBuilder();

        IEnumerable<FoodResponseDTO> foodResponseDtos = foods as FoodResponseDTO[] ?? foods.ToArray();
        if (foodResponseDtos.Count() == 1)
        {
            var f = foodResponseDtos.First();
            sb.AppendLine($"Dưới đây là thông tin dinh dưỡng về món ăn {f.Name}:");
            sb.AppendLine($"- Calories: {f.Calories} kcal");
            sb.AppendLine($"- Carbohydrates: {f.Carbs} g");
            sb.AppendLine($"- Fat: {f.Fat} g");
            sb.AppendLine($"- Protein: {f.Protein} g");
            sb.AppendLine();
            sb.AppendLine("Hãy viết một đoạn mô tả ngắn gọn và dễ hiểu về giá trị dinh dưỡng của món ăn này.");
        }
        else
        {
            sb.AppendLine("Dưới đây là thông tin dinh dưỡng về các loại thực phẩm:\n");

            foreach (var f in foodResponseDtos)
            {
                sb.AppendLine(
                    $"- {f.Name}: {f.Calories} kcal, {f.Carbs} g carbs, {f.Fat} g fat, {f.Protein} g protein");
            }

            sb.AppendLine();
            sb.AppendLine(
                "Hãy viết một đoạn mô tả so sánh ngắn gọn và dễ hiểu về sự khác biệt dinh dưỡng giữa các loại thực phẩm này.");
        }

        return sb.ToString();
    }
}