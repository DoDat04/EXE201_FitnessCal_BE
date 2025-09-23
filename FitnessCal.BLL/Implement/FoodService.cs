using FitnessCal.BLL.Constants;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.FoodDTO.Request;
using FitnessCal.BLL.DTO.FoodDTO.Response;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Text;


namespace FitnessCal.BLL.Implement;

public class FoodService : IFoodService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Supabase.Client _supabase;
    private readonly ILogger<FoodService> _logger;
    private readonly IGeminiService _geminiService;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IConfiguration configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FoodService(IUnitOfWork unitOfWork, ILogger<FoodService> logger, IGeminiService geminiService,
        IFoodRepository foodRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IChatMessageRepository chatMessageRepository)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _geminiService = geminiService;
        _foodRepository = foodRepository;
        _httpContextAccessor = httpContextAccessor;
        var supabaseUrl = configuration["Supabase:Url"];
        var supabaseKey = configuration["Supabase:Key"];

        _supabase = new Supabase.Client(supabaseUrl!, supabaseKey);
        _chatMessageRepository = chatMessageRepository;
    }

    public async Task<SearchFoodPaginationResponseDTO> SearchFoodsAsync(string? searchTerm = null, int page = 1,
        int pageSize = 15)
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

            _logger.LogInformation(
                "Food search completed. Page {Page}, Total: {TotalCount}, PageSize: {PageSize}, TotalPages: {TotalPages}",
                page, totalCount, pageSize, totalPages);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching foods. Page: {Page}, PageSize: {PageSize}", page,
                pageSize);
            throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
        }
    }

    public async Task<string> GenerateFoodsInformationAsync(string userPrompt)
    {
        Guid userId = GetCurrentUserId();
        var normalizedPrompt = TransformUserQuery(userPrompt);

        // 1. Tách input thành nhiều từ khóa nếu có dấu phẩy hoặc "và"
        var keywords = normalizedPrompt
            .Split(new[] { ",", " và ", "&" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .ToList();

        List<Food> foods = [];

        // 2. Nếu nhiều keyword → tìm từng cái
        if (keywords.Count > 1)
        {
            foreach (var key in keywords)
            {
                var matchedFoods = await _unitOfWork.Foods.FindAllAsync(
                    f => f.Name.ToLower() == key || f.Name.ToLower().Contains(key)
                );
                foods.AddRange(matchedFoods);
            }
        }
        else
        {
            // 2b. Nếu chỉ có 1 keyword → thử tìm chính xác trước
            var exactFood = await _unitOfWork.Foods.FindAsync(f => f.Name.ToLower() == normalizedPrompt);

            if (exactFood != null)
            {
                foods.Add(exactFood);
            }
            else
            {
                var matchedFoods = await _unitOfWork.Foods.FindAllAsync(
                    f => f.Name.ToLower().Contains(normalizedPrompt)
                );
                foods.AddRange(matchedFoods);
            }
        }

        // 3. Kiểm tra kết quả
        if (!foods.Any())
            throw new InvalidOperationException("Món ăn không có trong database. Vui lòng thử món khác.");

        // 4. Tạo DTO
        var dtoList = foods
            .DistinctBy(f => f.FoodId) // tránh trùng nếu search nhiều từ ra cùng 1 kết quả
            .Select(f => new FoodResponseDTO
            {
                FoodId = f.FoodId,
                Name = f.Name,
                Calories = f.Calories,
                Carbs = f.Carbs,
                Fat = f.Fat,
                Protein = f.Protein
            })
            .ToList();

        // 5. Tạo prompt phù hợp
        var prompt = GenerateFoodPrompt(dtoList);

        // 6. Gọi Gemini
        var aiResponse = await _geminiService.GenerateFoodsAsync(prompt);

        if (string.IsNullOrWhiteSpace(aiResponse))
            throw new InvalidOperationException("Gemini API không trả về dữ liệu hợp lệ.");

        var dailyId = await _chatMessageRepository.GetNextDailyIdAsync(userId);

        // 7. Lưu lịch sử chat
        var chatMessage = new ChatMessage
        {
            UserId = userId,
            DailyId = dailyId,
            UserPrompt = userPrompt,
            AiResponse = aiResponse,
            PromptTime = DateTime.UtcNow,
            ResponseTime = DateTime.UtcNow
        };
        await _chatMessageRepository.AddAsync(chatMessage);

        return aiResponse;
    }

    private static string GenerateFoodPrompt(IEnumerable<FoodResponseDTO> foods)
    {
        var sb = new StringBuilder();

        if (foods.Count() == 1)
        {
            var f = foods.First();
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

            foreach (var f in foods)
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
    
    private static string TransformUserQuery(string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
            return string.Empty;

        // 1. Chuẩn hóa
        var normalized = userPrompt.Trim().ToLower();

        // 2. Danh sách stopwords tiếng Việt (có thể mở rộng thêm)
        var stopwords = new List<string>
        {
            "hãy", "cho", "tôi", "xin", "thông tin", "về", "biết", "cung cấp", "là", "những", "các", "món", "ăn"
        };

        // 3. Loại bỏ stopwords
        foreach (var sw in stopwords)
        {
            normalized = normalized.Replace(sw, " ");
        }

        // 4. Chuẩn hóa khoảng trắng
        while (normalized.Contains("  "))
            normalized = normalized.Replace("  ", " ");

        return normalized.Trim();
    }
    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("UserId không tồn tại trong token");

        return Guid.Parse(userIdClaim.Value);
    }

    public async Task<ApiResponse<object>> UploadAndDetectFood(UploadFileRequest request, string prompt)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "No file uploaded",
                Data = null
            };
        }

        try
        {
            // 1. Upload file lên Supabase
            var fileName = string.IsNullOrWhiteSpace(request.File.FileName)
                ? Guid.NewGuid().ToString()
                : request.File.FileName;

            byte[] fileBytes;
            await using (var memoryStream = new MemoryStream())
            {
                await request.File.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var remotePath = $"fitnesscal-ai-detected/{fileName}";
            await _supabase.Storage
                .From("fitnesscal-storage")
                .Upload(fileBytes, remotePath, onProgress: (_, progress) =>
                    _logger.LogInformation($"{progress}% uploaded"));

            var publicUrl = _supabase.Storage
                .From("fitnesscal-storage")
                .GetPublicUrl(remotePath);

            // 2. Gọi Gemini
            var response = await _geminiService.GenerateTextFromImageAsync(publicUrl, prompt);
            if (string.IsNullOrWhiteSpace(response))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Không thể phân tích được món ăn từ ảnh",
                    Data = null
                };
            }

            // 3. Chuẩn hóa tên
            var foodName = response.Trim().ToLower();

            // 4. Check DB
            var food = await _unitOfWork.Foods.FirstOrDefaultAsync(f => f.Name.ToLower() == foodName);

            if (food != null)
            {
                var foodDto = new FoodResponseDTO
                {
                    FoodId = food.FoodId,
                    Name = food.Name,
                    Calories = food.Calories,
                    Carbs = food.Carbs,
                    Fat = food.Fat,
                    Protein = food.Protein
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Đã nhận diện thành công: {food.Name}",
                    Data = new
                    {
                        ImageUrl = publicUrl,
                        Food = foodDto
                    }
                };
            }

            // 5. Nếu không có trong DB
            return new ApiResponse<object>
            {
                Success = true,
                Message = "Text generated successfully from image using Gemini API",
                Data = new
                {
                    ImageUrl = publicUrl,
                    RawText = response
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload or detection failed");
            return new ApiResponse<object>
            {
                Success = false,
                Message = ResponseCodes.Messages.INTERNAL_ERROR,
                Data = null
            };
        }
    }

}