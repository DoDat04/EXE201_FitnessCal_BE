namespace FitnessCal.BLL.Define
{
    public interface IMealNotificationService
    {
        Task<bool> SendBreakfastNotificationToAllUsersAsync();
        Task<bool> SendLunchNotificationToAllUsersAsync();
        Task<bool> SendDinnerNotificationToAllUsersAsync();
    }
}
