namespace FitnessCal.BLL.DTO.FeedbacksDTO.Response
{
    public class FeedbacksResponseDTO
    {
        public int FeedbackId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RatingStars { get; set; }
        public string? Contribution { get; set; }
    }
}
