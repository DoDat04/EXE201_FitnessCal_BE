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
using FitnessCal.BLL.Transformer;
using FitnessCal.BLL.Helpers;

namespace FitnessCal.BLL.Implement;

public class FoodService : IFoodService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Supabase.Client _supabase;
    private readonly ILogger<FoodService> _logger;
    private readonly IGeminiService _geminiService;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TransformQueries _transformQueries;
    private readonly SaveTrainingData _saveTrainingData;
    private readonly CurrentUserIdHelper _currentUserIdHelper;

    public FoodService(IUnitOfWork unitOfWork, ILogger<FoodService> logger, IGeminiService geminiService, 
        IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
        IChatMessageRepository chatMessageRepository, CurrentUserIdHelper currentUserIdHelper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _geminiService = geminiService;
        _httpContextAccessor = httpContextAccessor;
        var supabaseUrl = configuration["Supabase:Url"];
        var supabaseKey = configuration["Supabase:Key"];

        _supabase = new Supabase.Client(supabaseUrl!, supabaseKey);
        _chatMessageRepository = chatMessageRepository;
        
        var classifyData = new ClassifyData(_geminiService);
        _saveTrainingData = new SaveTrainingData(_unitOfWork, classifyData, logger);
        _transformQueries = new TransformQueries(_geminiService);
        _currentUserIdHelper = currentUserIdHelper;
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
        var userId = _currentUserIdHelper.GetCurrentUserId();
        // Chuyển sang async version
        var normalizedPrompt = await _transformQueries.TransformUserQueryAsync(userPrompt);

        // 1. Tách keywords (sau khi đã lọc stopwords)
        var keywords = normalizedPrompt
            .Split([",", " và ", "&"], StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToList();

        List<Food> foods = [];
        List<PredefinedDish> dishes = [];

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

        var prompt = TransformQueries.GenerateFoodPrompt(promptDtos);

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
            var fileName = $"{_currentUserIdHelper.GetCurrentUserId()}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{request.File.FileName}";

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
            var prompt = TransformQueries.TransformQuery(publicUrl);

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

            // 4. Parse response từ AI (format: "Tên món|Calories|Carbs|Fat|Protein")
            var parsedFood = ParseAIResponse(response);
            if (parsedFood == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Không thể parse được thông tin dinh dưỡng từ AI",
                    Data = new { RawText = response }
                };
            }
            
            // Lưu món ăn mới vào DB để training
            await _saveTrainingData.SaveTrainingDataAsync(parsedFood);

            // 5. KHÔNG lưu vào UserCapturedFood ngay. Chỉ trả về bản xem trước để người dùng xác nhận.
            var preview = new
            {
                Name = parsedFood.Name,
                Calories = Math.Round(parsedFood.Calories, 1),
                Carbs = Math.Round(parsedFood.Carbs, 1),
                Fat = Math.Round(parsedFood.Fat, 1),
                Protein = Math.Round(parsedFood.Protein, 1)
            };

            // 6. Trả kết quả xem trước
            return new ApiResponse<object>
            {
                Success = true,
                Message = "Đã nhận diện món ăn từ ảnh. Vui lòng xác nhận để lưu.",
                Data = new
                {
                    ImageUrl = publicUrl,
                    DetectedFood = preview,
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

    public async Task<AddCapturedFoodToMealResponseDTO> AddCapturedFoodToMealAsync(AddCapturedFoodToMealRequestDTO request)
    {
        try
        {
            var userId = _currentUserIdHelper.GetCurrentUserId();

            // 1. Kiểm tra UserCapturedFood có tồn tại và thuộc về user hiện tại
            var capturedFood = await _unitOfWork.UserCapturedFoods.GetByIdAsync(request.CapturedFoodId);
            if (capturedFood == null)
            {
                throw new KeyNotFoundException("Không tìm thấy món ăn đã chụp");
            }

            if (capturedFood.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập món ăn này");
            }

            // 2. Kiểm tra MealLog có tồn tại và thuộc về user hiện tại
            var mealLog = await _unitOfWork.UserMealLogs.GetByIdAsync(request.MealLogId);
            if (mealLog == null)
            {
                throw new KeyNotFoundException("Không tìm thấy bữa ăn");
            }

            if (mealLog.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập bữa ăn này");
            }

            // 3. Tính calories theo số lượng
            var totalCalories = capturedFood.Calories * request.Quantity;

            // 4. Tạo UserMealItem
            var mealItem = new UserMealItem
            {
                LogId = request.MealLogId,
                IsCustom = 0,
                FoodId = null,
                DishId = null,
                UserCapturedFoodId = request.CapturedFoodId, 
                Quantity = request.Quantity,
                Calories = totalCalories
            };

            await _unitOfWork.UserMealItems.AddAsync(mealItem);
            var result = await _unitOfWork.Save();

            if (!result)
            {
                throw new Exception("Không thể lưu món ăn vào bữa ăn");
            }

            _logger.LogInformation("Captured food {CapturedFoodId} added to meal log {MealLogId} successfully", 
                request.CapturedFoodId, request.MealLogId);

            return new AddCapturedFoodToMealResponseDTO
            {
                ItemId = mealItem.ItemId,
                MealLogId = request.MealLogId,
                CapturedFoodId = request.CapturedFoodId,
                FoodName = capturedFood.Name,
                Quantity = request.Quantity,
                Calories = totalCalories,
                Message = "Thêm món ăn vào bữa ăn thành công"
            };
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding captured food {CapturedFoodId} to meal log {MealLogId}", 
                request.CapturedFoodId, request.MealLogId);
            throw new Exception("Có lỗi xảy ra khi thêm món ăn vào bữa ăn");
        }
    }

    public async Task<List<GetUserCapturedFoodsResponseDTO>> GetUserCapturedFoodsAsync()
    {
        try
        {
            var userId = _currentUserIdHelper.GetCurrentUserId();

            var userCapturedFoods = await _unitOfWork.UserCapturedFoods.GetAllAsync(ucf => ucf.UserId == userId);
            
            var result = userCapturedFoods.Select(ucf => new GetUserCapturedFoodsResponseDTO
            {
                Id = ucf.Id,
                Name = ucf.Name,
                Calories = ucf.Calories,
                Carbs = ucf.Carbs,
                Fat = ucf.Fat,
                Protein = ucf.Protein
            }).ToList();

            _logger.LogInformation("Retrieved {Count} user captured foods for user {UserId}", result.Count, userId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user captured foods");
            throw new Exception("Có lỗi xảy ra khi lấy danh sách món ăn đã chụp");
        }
    }
    private static ParsedFoodInfo? ParseAIResponse(string response)
    {
        try
        {
            // Lấy dòng đầu tiên và loại bỏ ký tự đặc biệt
            var cleanResponse = response.Split('\n').FirstOrDefault()?.Trim();
            if (string.IsNullOrWhiteSpace(cleanResponse))
                return null;

            // Loại bỏ ký tự đặc biệt
            cleanResponse = cleanResponse.Replace("*", "").Replace("-", "").Trim();

            // Parse format: "Tên món|Calories|Carbs|Fat|Protein"
            var parts = cleanResponse.Split('|');
            if (parts.Length != 5)
                return null;

            var name = parts[0].Trim();
            if (double.TryParse(parts[1].Trim(), out var calories) &&
                double.TryParse(parts[2].Trim(), out var carbs) &&
                double.TryParse(parts[3].Trim(), out var fat) &&
                double.TryParse(parts[4].Trim(), out var protein))
            {
                return new ParsedFoodInfo
                {
                    Name = name,
                    Calories = calories,
                    Carbs = carbs,
                    Fat = fat,
                    Protein = protein
                };
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
    public class ParsedFoodInfo
    {
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double Protein { get; set; }
    }

    public async Task<ApiResponse<ConfirmCapturedFoodResponseDTO>> ConfirmCapturedFoodAsync(ConfirmCapturedFoodRequestDTO request)
    {
        try
        {
            var userId = _currentUserIdHelper.GetCurrentUserId();

            var userCapturedFood = new UserCapturedFood
            {
                UserId = userId,
                Name = request.Name,
                Calories = request.Calories,
                Carbs = request.Carbs,
                Fat = request.Fat,
                Protein = request.Protein
            };

            await _unitOfWork.UserCapturedFoods.AddAsync(userCapturedFood);
            var saved = await _unitOfWork.Save();

            if (!saved)
            {
                throw new Exception("Không thể lưu món ăn đã nhận diện");
            }

            var response = new ConfirmCapturedFoodResponseDTO
            {
                CapturedFoodId = userCapturedFood.Id,
                Name = userCapturedFood.Name,
                Calories = userCapturedFood.Calories,
                Carbs = userCapturedFood.Carbs,
                Fat = userCapturedFood.Fat,
                Protein = userCapturedFood.Protein
            };

            return new ApiResponse<ConfirmCapturedFoodResponseDTO>
            {
                Success = true,
                Message = "Đã lưu món ăn đã nhận diện",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming captured food");
            return new ApiResponse<ConfirmCapturedFoodResponseDTO>
            {
                Success = false,
                Message = ResponseCodes.Messages.INTERNAL_ERROR,
                Data = null
            };
        }
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
        if (foods.Count == 0)
            return [];

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

    public async Task<GetUserCapturedFoodDetailsResponseDTO> GetUserCapturedFoodDetailsAsync(int id)
    {
        try
        {
            var userId = _currentUserIdHelper.GetCurrentUserId();
            var userCapturedFood = await _unitOfWork.UserCapturedFoods.GetByIdAsync(id);
            
            if (userCapturedFood == null)
            {
                throw new KeyNotFoundException("UserCapturedFood not found");
            }

            // Kiểm tra quyền truy cập - chỉ user sở hữu mới được xem
            if (userCapturedFood.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập món ăn này");
            }

            return new GetUserCapturedFoodDetailsResponseDTO
            {
                Id = userCapturedFood.Id,
                Name = userCapturedFood.Name,
                Calories = userCapturedFood.Calories,
                Carbs = userCapturedFood.Carbs,
                Fat = userCapturedFood.Fat,
                Protein = userCapturedFood.Protein,
                SourceType = "UserCapturedFood"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user captured food details for id {Id}", id);
            throw;
        }
    }
}