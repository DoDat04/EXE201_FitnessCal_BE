using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FoodDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using Microsoft.AspNetCore.Authorization;
using FitnessCal.BLL.DTO.FoodDTO.Request;

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

        public FoodController(IFoodService foodService, ILogger<FoodController> logger, IConfiguration configuration)
        {
            _foodService = foodService;
            _logger = logger;
            var supabaseUrl = configuration["Supabase:Url"];
            var supabaseKey = configuration["Supabase:Key"];

            _supabase = new Supabase.Client(supabaseUrl!, supabaseKey);
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
    }
}
