namespace FitnessCal.BLL.DTO.DashboardDTO.Response
{
    public class DailyRevenueDTO
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int SubscriptionCount { get; set; }
    }
}
