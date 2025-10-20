using FitnessCal.BLL.DTO.FeedbacksDTO.Request;
using FitnessCal.BLL.DTO.FeedbacksDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IFeedbacksService
    {
        Task<CreateFeedbackResponseDTO> CreateFeedbackAsync(CreateFeedbackRequestDTO feedback);
        Task<UpdateFeedbackResponseDTO> UpdateFeedbackAsync(int id, UpdateFeedbackRequestDTO feedback);
        Task<bool> DeleteFeedbackAsync(int id);
        Task<IEnumerable<FeedbacksResponseDTO>> GetAllFeedbacksAsync(int? stars, DateTime? searchDate);
        Task<FeedbacksResponseDTO> GetFeedbackByIdAsync(Guid userId);
        Task<double> AverageRatingStarsAsync();
    }
}
