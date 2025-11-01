using Azure.Core;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FeedbacksDTO.Request;
using FitnessCal.BLL.DTO.FeedbacksDTO.Response;
using FitnessCal.BLL.Helpers;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.BLL.Implement
{
    public class FeedbacksService : IFeedbacksService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrentUserIdHelper _currentUserIdHelper;

        public FeedbacksService(IUnitOfWork unitOfWork, CurrentUserIdHelper currentUserIdHelper)
        {
            _unitOfWork = unitOfWork;
            _currentUserIdHelper = currentUserIdHelper;
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
                    !stars.HasValue || f.RatingStars == stars.Value, f=> f.User
                );
            if (searchDate.HasValue)
            {
                var localDate = searchDate.Value.Date;
                feedbacksQuery = feedbacksQuery
                    .Where(f => f.CreatedAt.ToLocalTime().Date == localDate)
                    .ToList();
            }

            var orderedFeedbacks = feedbacksQuery
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            return orderedFeedbacks.Select(f => new FeedbacksResponseDTO
            {
                FeedbackId = f.FeedbackId,
                UserId = f.UserId,
                UserName = f.User.FirstName + " " + f.User.LastName,
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

        public async Task<UpdateFeedbackResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackRequestDTO request)
        {
            var currentUserId = _currentUserIdHelper.GetCurrentUserId();
            var feedback = await _unitOfWork.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId && f.UserId == currentUserId);

            if (feedback == null)
            {
                throw new KeyNotFoundException("Feedback không tồn tại hoặc bạn không có quyền sửa.");
            }

            feedback.RatingStars = request.RatingStars;
            feedback.Contribution = request.Contribution;

            await _unitOfWork.Feedbacks.UpdateAsync(feedback);
            await _unitOfWork.Save();

            return new UpdateFeedbackResponseDTO
            {
                FeedbackId = feedback.FeedbackId,
                UserId = feedback.UserId,
                CreatedAt = feedback.CreatedAt,
                RatingStars = feedback.RatingStars,
                Contribution = feedback.Contribution
            };
        }
        public async Task<bool> DeleteFeedbackAsync(int feedbackId)
        {
            var currentUserId = _currentUserIdHelper.GetCurrentUserId();
            var feedback = await _unitOfWork.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId && f.UserId == currentUserId);
            if (feedback == null)
            {
                throw new KeyNotFoundException("Feedback không tồn tại hoặc bạn không có quyền xóa.");
            }
            await _unitOfWork.Feedbacks.DeleteAsync(feedback);
            await _unitOfWork.Save();
            return true;
        }

        public async Task<IEnumerable<FeedbacksResponseDTO>> GetFeedbackByIdAsync(Guid userId)
        {
            var currentUserId = _currentUserIdHelper.GetCurrentUserId();
            if (userId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập phản hồi này.");
            }

            var feedbacks = await _unitOfWork.Feedbacks.GetAllAsync(f => f.UserId == userId);
            return feedbacks.Select(f => new FeedbacksResponseDTO
            {
                FeedbackId = f.FeedbackId,
                UserId = f.UserId,
                CreatedAt = f.CreatedAt,
                RatingStars = f.RatingStars,
                Contribution = f.Contribution
            }).ToList();
        }

    }
}
