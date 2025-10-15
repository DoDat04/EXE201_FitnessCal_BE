namespace FitnessCal.BLL.DTO.DashboardDTO.Response
{
    public class MonthlyRevenueDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int SubscriptionCount { get; set; }
    }
}
