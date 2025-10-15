using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FeedbacksDTO.Request;
using FitnessCal.BLL.DTO.FeedbacksDTO.Response;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.BLL.Implement
{
    public class FeedbacksService : IFeedbacksService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeedbacksService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateFeedbackResponseDTO> CreateFeedbackAsync(CreateFeedbackRequestDTO feedback)
        {
            var newFeedback = new Feedbacks
            {
                UserId = feedback.UserId,
                RatingStars = feedback.RatingStars,
                Contribution = feedback.Contribution,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Feedbacks.AddAsync(newFeedback);
            await _unitOfWork.Save();

            return new CreateFeedbackResponseDTO
            {
                FeedbackId = newFeedback.FeedbackId,
                UserId = newFeedback.UserId,
                CreatedAt = newFeedback.CreatedAt,
                RatingStars = newFeedback.RatingStars,
                Contribution = newFeedback.Contribution
            };
        }
        public async Task<IEnumerable<FeedbacksResponseDTO>> GetAllFeedbacksAsync()
        {
            var feedbacks = await _unitOfWork.Feedbacks.GetAllAsync();

            var feedbackList = feedbacks.Select(f => new FeedbacksResponseDTO
            {
                FeedbackId = f.FeedbackId,
                UserId = f.UserId,
                CreatedAt = f.CreatedAt,
                RatingStars = f.RatingStars,
                Contribution = f.Contribution
            }).ToList();

            return feedbackList;
        }

    }
}
