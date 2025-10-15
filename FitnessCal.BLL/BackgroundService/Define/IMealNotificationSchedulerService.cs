namespace FitnessCal.BLL.BackgroundService.Define
{
    public interface IMealNotificationSchedulerService
    {
        Task ProcessMealNotificationsAsync();
        Task SendBreakfastNotificationsAsync();
        Task SendLunchNotificationsAsync();
        Task SendDinnerNotificationsAsync();
        Task<bool> ShouldSendNotificationAsync(int currentHour);
        Task<string> GetMealTypeForHourAsync(int hour);
    }
}
