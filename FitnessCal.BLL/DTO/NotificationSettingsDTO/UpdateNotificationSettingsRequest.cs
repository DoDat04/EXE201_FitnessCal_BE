using System.ComponentModel.DataAnnotations;

namespace FitnessCal.BLL.DTO.NotificationSettingsDTO
{
    public class UpdateNotificationSettingsRequest
    {
        [Required]
        public bool IsNotificationEnabled { get; set; }

        [Required]
        public bool BreakfastNotification { get; set; }

        [Required]
        public bool LunchNotification { get; set; }

        [Required]
        public bool DinnerNotification { get; set; }
    }
}
