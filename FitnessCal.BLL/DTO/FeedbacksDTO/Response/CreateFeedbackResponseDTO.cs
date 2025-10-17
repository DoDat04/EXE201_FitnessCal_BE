namespace FitnessCal.BLL.DTO.FeedbacksDTO.Response
{
    public class CreateFeedbackResponseDTO
    {
        public int FeedbackId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RatingStars { get; set; }
        public string? Contribution { get; set; } = null;
    }
}
