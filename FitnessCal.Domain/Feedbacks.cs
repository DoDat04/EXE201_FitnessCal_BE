namespace FitnessCal.Domain
{
    public class Feedbacks
    {
        public int FeedbackId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RatingStars { get; set; }
        public string? Contribution { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
