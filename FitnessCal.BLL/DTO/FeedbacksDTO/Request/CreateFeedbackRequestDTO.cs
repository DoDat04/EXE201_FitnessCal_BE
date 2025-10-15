namespace FitnessCal.BLL.DTO.FeedbacksDTO.Request
{
    public class CreateFeedbackRequestDTO
    {
        public Guid UserId { get; set; }
        public int RatingStars { get; set; }
        public string? Contribution { get; set; }
    }
}
