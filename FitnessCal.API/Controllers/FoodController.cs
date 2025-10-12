using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FoodDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using Microsoft.AspNetCore.Authorization;
using FitnessCal.BLL.DTO.FoodDTO.Request;
using FitnessCal.DAL.Define;

namespace FitnessCal.API.Controllers
{
    [Route("api/foods")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;
        private readonly ILogger<FoodController> _logger;
        private readonly Supabase.Client _supabase;
        private readonly IGeminiService _geminiService;
        private readonly IUnitOfWork _unitOfWork;

        public FoodController(IFoodService foodService, ILogger<FoodController> logger, IConfiguration configuration, IGeminiService geminiService, IUnitOfWork unitOfWork)
        {
            _foodService = foodService;
            _logger = logger;
            var supabaseUrl = configuration["Supabase:Url"];
            var supabaseKey = configuration["Supabase:Key"];

            _supabase = new Supabase.Client(supabaseUrl!, supabaseKey);
            _geminiService = geminiService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            var file = request.File;

            if (file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            try
            {
                var fileName = string.IsNullOrWhiteSpace(file.FileName)
                    ? Guid.NewGuid().ToString()
                    : file.FileName;

                byte[] fileBytes;
                await using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var remotePath = $"foods/{fileName}";
                await _supabase.Storage
                    .From("fitnesscal-storage")
                    .Upload(fileBytes, remotePath, onProgress: (_, progress) =>
                        _logger.LogInformation($"{progress}% uploaded"));

                var publicUrl = _supabase.Storage
                    .From("fitnesscal-storage")
                    .GetPublicUrl(remotePath);

                return Ok(new
                {
                    message = "File uploaded successfully",
                    fileName,
                    url = publicUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<SearchFoodPaginationResponseDTO>>> SearchFoods([FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
        {
            try
            {
                var searchResults = await _foodService.SearchFoodsAsync(search, page, pageSize);

                string message;
                if (string.IsNullOrWhiteSpace(search))
                {
                    message = searchResults.Foods.Any()
                        ? $"Hiển thị trang {page}/{searchResults.TotalPages} với {searchResults.Foods.Count()} món ăn phổ biến"
                        : "Không có món ăn nào trong database";
                }
                else
                {
                    message = searchResults.Foods.Any()
                        ? $"Tìm thấy {searchResults.TotalCount} món ăn với từ khóa '{search}'. Trang {page}/{searchResults.TotalPages}"
                        : $"Không tìm thấy món ăn nào với từ khóa '{search}'";
                }

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<SearchFoodPaginationResponseDTO>
                {
                    Success = true,
                    Message = message,
                    Data = searchResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching foods");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<SearchFoodPaginationResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
        [HttpPost("generate-foods-info")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateFoodsInformation([FromBody] string request)
        {
            try
            {
                var response = await _foodService.GenerateFoodsInformationAsync(request);
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<string>
                {
                    Success = true,
                    Message = "Thực phẩm được tạo thành công từ Gemini API",
                    Data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while generating foods information");
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<FoodResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating foods information");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<FoodResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("upload-and-detect")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<object>>> UploadAndDetectFood([FromForm] UploadFileRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No file uploaded",
                    Data = null
                });
            }

            try
            {
                // gọi xuống service (không cần prompt nữa)
                var result = await _foodService.UploadAndDetectFood(request);

                // Nếu service xử lý thành công hoặc trả về lỗi soft → luôn trả 200
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload or detection failed");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = ResponseCodes.Messages.INTERNAL_ERROR,
                        Data = null
                    });
            }
        }


        [HttpGet("details/{id}")]
        public async Task<ActionResult<ApiResponse<SearchFoodResponseDTO>>> GetFoodDetails(int id, [FromQuery] string type)
        {
            try
            {
                // Validate type parameter
                if (string.IsNullOrWhiteSpace(type) || (type != "Food" && type != "PredefinedDish"))
                {
                    return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<SearchFoodResponseDTO>
                    {
                        Success = false,
                        Message = "Invalid type parameter. Must be 'Food' or 'PredefinedDish'",
                        Data = null
                    });
                }

                var result = await _foodService.GetFoodDetailsAsync(id, type);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<SearchFoodResponseDTO>
                {
                    Success = true,
                    Message = $"Lấy thông tin chi tiết {type} thành công",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Food or Dish not found with id {Id} and type {Type}", id, type);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<SearchFoodResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in GetFoodDetails: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<SearchFoodResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting food details for id {Id} and type {Type}", id, type);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<SearchFoodResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpGet("search-food-by-name")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FoodResponseDTO?>>>> SearchFoodByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<FoodResponseDTO>
                    {
                        Success = false,
                        Message = "Name parameter is required",
                        Data = null
                    });
                }

                var food = await _foodService.SearchFoodByNameAsync(name);

                var foodDto = food.Select(f =>
                {
                    if (f != null)
                        return new FoodResponseDTO
                        {
                            FoodId = f.FoodId,
                            Name = f.Name,
                            Calories = f.Calories,
                            Carbs = f.Carbs,
                            Fat = f.Fat,
                            Protein = f.Protein
                        };
                    return null;
                });

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<IEnumerable<FoodResponseDTO?>>
                {
                    Success = true,
                    Message = $"Found food with name '{name}'",
                    Data = foodDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching food by name '{Name}'", name);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<FoodResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("add-multiple")]
        public async Task<ActionResult<ApiResponse<List<AddFoodResponseDTO>>>> AddMultipleFoods([FromBody] List<AddFoodRequestDTO> foods)
        {
            try
            {
                if (foods == null || !foods.Any())
                {
                    return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<List<AddFoodResponseDTO>>
                    {
                        Success = false,
                        Message = "Food list cannot be empty",
                        Data = null
                    });
                }
                var addedFoods = await _foodService.AddFoodInformationAsync(foods);
                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<List<AddFoodResponseDTO>>
                {
                    Success = true,
                    Message = $"{addedFoods.Count} foods added successfully",
                    Data = addedFoods
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in AddMultipleFoods: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<List<AddFoodResponseDTO>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding multiple foods");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<List<AddFoodResponseDTO>>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
    }
}
