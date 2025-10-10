namespace FitnessCal.BLL.DTO.NotificationSettingsDTO
{
    public class UserNotificationSettingsDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool IsNotificationEnabled { get; set; }
        public bool BreakfastNotification { get; set; }
        public bool LunchNotification { get; set; }
        public bool DinnerNotification { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
