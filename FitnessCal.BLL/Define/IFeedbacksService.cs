using FitnessCal.BLL.DTO.FeedbacksDTO.Request;
using FitnessCal.BLL.DTO.FeedbacksDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IFeedbacksService
    {
        Task<CreateFeedbackResponseDTO> CreateFeedbackAsync(CreateFeedbackRequestDTO feedback);
        Task<IEnumerable<FeedbacksResponseDTO>> GetAllFeedbacksAsync();
    }
}
