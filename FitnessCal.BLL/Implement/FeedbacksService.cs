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
        public async Task<IEnumerable<FeedbacksResponseDTO>> GetAllFeedbacksAsync(int? stars, DateTime? searchDate)
        {
            var feedbacksQuery = await _unitOfWork.Feedbacks.GetAllAsync(f =>
                    !stars.HasValue || f.RatingStars == stars.Value
                );

            if (searchDate.HasValue)
            {
                var localDate = searchDate.Value.Date;
                feedbacksQuery = feedbacksQuery
                    .Where(f => f.CreatedAt.ToLocalTime().Date == localDate)
                    .ToList();
            }

            return feedbacksQuery.Select(f => new FeedbacksResponseDTO
            {
                FeedbackId = f.FeedbackId,
                UserId = f.UserId,
                CreatedAt = f.CreatedAt,
                RatingStars = f.RatingStars,
                Contribution = f.Contribution
            }).ToList();
        }
        public async Task<double> AverageRatingStarsAsync()
        {
            var feedbacks = await _unitOfWork.Feedbacks.GetAllAsync();
            if (!feedbacks.Any())
            {
                return 0;
            }
            var averageRating = (double)feedbacks.Average(f => f.RatingStars);
            return averageRating;
        }
    }
}
