namespace FitnessCal.BLL.DTO.CommonDTO
{
    public class MealNotificationSettings
    {
        public int BreakfastHour { get; set; } = 7;
        public int LunchHour { get; set; } = 12;
        public int DinnerHour { get; set; } = 18;
        public int CheckIntervalMinutes { get; set; } = 1;
        public bool EnableNotifications { get; set; } = true;
        public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    }
}
