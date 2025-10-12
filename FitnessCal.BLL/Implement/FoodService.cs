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
        IFoodRepository foodRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
        IChatMessageRepository chatMessageRepository)
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

    public async Task<IEnumerable<Food?>> SearchFoodByNameAsync(string name)
    {
        return await _unitOfWork.Foods.FirstOrDefaultAsync(f => f.Name.ToLower() == name.ToLower()) != null
            ? new List<Food?> { await _unitOfWork.Foods.FirstOrDefaultAsync(f => f.Name.ToLower() == name.ToLower()) }!
            : await _unitOfWork.Foods.FindAllAsync(f => f.Name.ToLower().Contains(name.ToLower()));
    }

    public async Task<IEnumerable<PredefinedDish?>> SearchPredefinedDishByNameAsync(string name)
    {
        return await _unitOfWork.PredefinedDishes.FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower()) != null
            ? new List<PredefinedDish?> { await _unitOfWork.PredefinedDishes.FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower()) }!
            : await _unitOfWork.PredefinedDishes.FindAllAsync(d => d.Name.ToLower().Contains(name.ToLower()));
    }

    public async Task<string> GenerateFoodsInformationAsync(string userPrompt)
    {
        Guid userId = GetCurrentUserId();
        var normalizedPrompt = TransformUserQuery(userPrompt);

        // 1. Tách keywords
        var keywords = normalizedPrompt
            .Split(new[] { ",", " và ", "&" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .ToList();

        List<Food> foods = new();
        List<PredefinedDish> dishes = new();

        // 2. Tìm kiếm foods và predefined dishes
        if (keywords.Count > 1)
        {
            foreach (var key in keywords)
            {
                var matchedFoods = await _unitOfWork.Foods.FindAllAsync(
                    f => f.Name.ToLower() == key.ToLower() || f.Name.ToLower().Contains(key.ToLower())
                );
                foods.AddRange(matchedFoods);

                var matchedDishes = await _unitOfWork.PredefinedDishes.FindAllAsync(
                    d => d.Name.ToLower() == key.ToLower() || d.Name.ToLower().Contains(key.ToLower())
                );
                dishes.AddRange(matchedDishes);
            }
        }
        else
        {
            var exactFood = await _unitOfWork.Foods.FindAsync(f => f.Name.ToLower() == normalizedPrompt.ToLower());
            if (exactFood != null) foods.Add(exactFood);
            else
            {
                var matchedFoods = await _unitOfWork.Foods.FindAllAsync(
                    f => f.Name.ToLower().Contains(normalizedPrompt.ToLower())
                );
                foods.AddRange(matchedFoods);
            }

            var exactDish = await _unitOfWork.PredefinedDishes.FindAsync(d => d.Name.ToLower() == normalizedPrompt.ToLower());
            if (exactDish != null) dishes.Add(exactDish);
            else
            {
                var matchedDishes = await _unitOfWork.PredefinedDishes.FindAllAsync(
                    d => d.Name.ToLower().Contains(normalizedPrompt.ToLower())
                );
                dishes.AddRange(matchedDishes);
            }
        }

        if (!foods.Any() && !dishes.Any())
            throw new InvalidOperationException("Món ăn không có trong database. Vui lòng thử món khác.");

        // 3. Chuyển sang SearchFoodResponseDTO
        var searchDtos = foods
            .DistinctBy(f => f.FoodId)
            .Select(f => new SearchFoodResponseDTO
            {
                Id = f.FoodId,
                Name = f.Name,
                Calories = f.Calories,
                Carbs = f.Carbs,
                Fat = f.Fat,
                Protein = f.Protein,
                ServingUnit = null,
                SourceType = "Food",
                FoodId = f.FoodId,
                DishId = null
            })
            .ToList();

        searchDtos.AddRange(dishes
            .DistinctBy(d => d.DishId)
            .Select(d => new SearchFoodResponseDTO
            {
                Id = d.DishId,
                Name = d.Name,
                Calories = d.Calories,
                Carbs = d.Carbs,
                Fat = d.Fat,
                Protein = d.Protein,
                ServingUnit = d.ServingUnit,
                SourceType = "PredefinedDish",
                FoodId = null,
                DishId = d.DishId
            }));

        // 4. Chuyển sang FoodResponseDTO để tạo prompt AI
        var promptDtos = searchDtos.Select(d => new FoodResponseDTO
        {
            FoodId = d.FoodId ?? d.DishId ?? 0,
            Name = d.Name,
            Calories = d.Calories,
            Carbs = d.Carbs,
            Fat = d.Fat,
            Protein = d.Protein
        }).ToList();

        var prompt = GenerateFoodPrompt(promptDtos);

        var aiResponse = await _geminiService.GenerateFoodsAsync(prompt);

        if (string.IsNullOrWhiteSpace(aiResponse))
            throw new InvalidOperationException("Gemini API không trả về dữ liệu hợp lệ.");

        // 5. Lưu lịch sử chat
        var today = DateTime.UtcNow.Date;
        var chatMessage = await _chatMessageRepository.GetByUserAndDateAsync(userId, today);

        if (chatMessage == null)
        {
            chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChatDate = today,
                DailyMessages = new List<DailyMessage>()
            };
        }

        int dailyId = chatMessage.DailyMessages.Count + 1;

        chatMessage.DailyMessages.Add(new DailyMessage
        {
            DailyId = dailyId,
            UserPrompt = userPrompt,
            AiResponse = aiResponse,
            PromptTime = DateTime.UtcNow,
            ResponseTime = DateTime.UtcNow
        });

        await _chatMessageRepository.UpsertAsync(chatMessage);

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

    public async Task<ApiResponse<object>> UploadAndDetectFood(UploadFileRequest request)
    {
        if (request.File.Length == 0)
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
            var fileName = $"{GetCurrentUserId()}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{request.File.FileName}";

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

            // 2. Transform query từ ảnh
            string prompt = TransformQuery(publicUrl);

            // 3. Gọi Gemini AI → text mô tả đồ ăn
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

            // 4. Chuẩn hóa & tách text → nhiều món ăn
            var detectedFoods = response
                .Split(new[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Replace("*", "").Replace("-", "").Trim().ToLower())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (!detectedFoods.Any())
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Không tìm thấy món ăn hợp lệ trong văn bản AI trả về",
                    Data = new { RawText = response }
                };
            }

            // 5. Check DB & cộng dinh dưỡng
            double totalCalories = 0, totalCarbs = 0, totalFat = 0, totalProtein = 0;
            var foundFoods = new List<SearchFoodResponseDTO>();
            var notFoundFoods = new List<string>();

            foreach (var name in detectedFoods)
            {
                var foods = await SearchFoodByNameAsync(name);
                var dishes = await SearchPredefinedDishByNameAsync(name);

                if (foods.Any())
                {
                    foreach (var f in foods)
                    {
                        totalCalories += f!.Calories;
                        totalCarbs += f.Carbs;
                        totalFat += f.Fat;
                        totalProtein += f.Protein;

                        foundFoods.Add(new SearchFoodResponseDTO
                        {
                            Id = f.FoodId,
                            Name = f.Name,
                            Calories = f.Calories,
                            Carbs = f.Carbs,
                            Fat = f.Fat,
                            Protein = f.Protein,
                            ServingUnit = null,
                            SourceType = "Food",
                            FoodId = f.FoodId,
                            DishId = null
                        });
                    }
                }
                else if (dishes.Any())
                {
                    foreach (var d in dishes)
                    {
                        totalCalories += d!.Calories;
                        totalCarbs += d.Carbs;
                        totalFat += d.Fat;
                        totalProtein += d.Protein;

                        foundFoods.Add(new SearchFoodResponseDTO
                        {
                            Id = d.DishId,
                            Name = d.Name,
                            Calories = d.Calories,
                            Carbs = d.Carbs,
                            Fat = d.Fat,
                            Protein = d.Protein,
                            ServingUnit = d.ServingUnit,
                            SourceType = "PredefinedDish",
                            FoodId = null,
                            DishId = d.DishId
                        });
                    }
                }
                else
                {
                    notFoundFoods.Add(name);
                }
            }

            // 6. Trả kết quả
            return new ApiResponse<object>
            {
                Success = true,
                Message = foundFoods.Any()
                    ? "Đã nhận diện thành công một số món ăn"
                    : "Không tìm thấy món ăn nào trong DB",
                Data = new
                {
                    ImageUrl = publicUrl,
                    Foods = foundFoods,
                    TotalNutrition = new
                    {
                        Calories = totalCalories,
                        Carbs = totalCarbs,
                        Fat = totalFat,
                        Protein = totalProtein
                    },
                    NotFound = notFoundFoods,
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

    private static string TransformQuery(string imageUrl)
    {
        return $@"
            Phân tích món ăn trong ảnh sau: {imageUrl}.
            Nhiệm vụ:
            1. Nhận diện tất cả món ăn có trong ảnh.
            2. Trả về danh sách tên món ăn (ngắn gọn, rõ ràng).
            3. Nếu có nhiều món, hãy liệt kê theo từng dòng.
            4. Không giải thích dài dòng, chỉ liệt kê tên món ăn để dùng cho tra cứu DB.
            ";
    }

    public async Task<SearchFoodResponseDTO> GetFoodDetailsAsync(int id, string type)
    {
        try
        {
            if (type == "Food")
            {
                var food = await _unitOfWork.Foods.GetByIdAsync(id);
                if (food == null)
                {
                    throw new KeyNotFoundException("Food not found");
                }

                return new SearchFoodResponseDTO
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
                };
            }
            else if (type == "PredefinedDish")
            {
                var dish = await _unitOfWork.PredefinedDishes.GetByIdAsync(id);
                if (dish == null)
                {
                    throw new KeyNotFoundException("PredefinedDish not found");
                }

                return new SearchFoodResponseDTO
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
                };
            }
            else
            {
                throw new ArgumentException("Invalid type. Must be 'Food' or 'PredefinedDish'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting food details for id {Id} and type {Type}", id, type);
            throw;
        }
    }
    public async Task<List<AddFoodResponseDTO>> AddFoodInformationAsync(List<AddFoodRequestDTO> foods)
    {
        if (foods == null || !foods.Any())
            return new List<AddFoodResponseDTO>();

        // Chuyển đổi DTO sang entity, **không gán FoodId**
        var foodEntities = foods.Select(f => new Food
        {
            Name = f.Name,
            Calories = f.Calories,
            Carbs = f.Carbs,
            Fat = f.Fat,
            Protein = f.Protein,
            FoodCategory = f.FoodCategory
        }).ToList();

        // Thêm nhiều entity cùng lúc
        await _unitOfWork.Foods.AddRangeAsync(foodEntities);

        // Lưu thay đổi, EF Core sẽ tự điền FoodId từ DB
        await _unitOfWork.Save();

        // Chuyển entity vừa lưu sang DTO, FoodId đã có giá trị từ DB
        var response = foodEntities.Select(f => new AddFoodResponseDTO
        {
            FoodId = f.FoodId,
            Name = f.Name,
            Calories = f.Calories,
            Carbs = f.Carbs,
            Fat = f.Fat,
            Protein = f.Protein,
            FoodCategory = f.FoodCategory
        }).ToList();

        return response;
    }
}