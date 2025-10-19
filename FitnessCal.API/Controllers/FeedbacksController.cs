using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FeedbacksDTO.Request;
using FitnessCal.BLL.DTO.FeedbacksDTO.Response;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbacksService _feedbacksService;
        public FeedbacksController(IFeedbacksService feedbacksService)
        {
            _feedbacksService = feedbacksService;
        }
        [HttpPost]
        public async Task<ActionResult<CreateFeedbackResponseDTO>> CreateFeedback([FromBody] CreateFeedbackRequestDTO feedback)
        {
            if (feedback == null)
                return BadRequest("Feedback request không hợp lệ.");

            try
            {
                var createdFeedback = await _feedbacksService.CreateFeedbackAsync(feedback);

                if (createdFeedback == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Không thể tạo feedback.");

                return CreatedAtAction(nameof(CreateFeedback), new { id = createdFeedback.FeedbackId }, createdFeedback);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IEnumerable<FeedbacksResponseDTO>> GetAllFeedbacks(int? stars, DateTime? searchDate)
        {
            try
            {
                var feedbacks = await _feedbacksService.GetAllFeedbacksAsync(stars, searchDate);
                return feedbacks;
            }
            catch (Exception)
            {
                return Enumerable.Empty<FeedbacksResponseDTO>();
            }
        }
        [HttpGet("average-rating")]
        public async Task<ActionResult<double>> GetAverageRating()
        {
            try
            {
                var averageRating = await _feedbacksService.AverageRatingStarsAsync();
                return Ok(averageRating);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
