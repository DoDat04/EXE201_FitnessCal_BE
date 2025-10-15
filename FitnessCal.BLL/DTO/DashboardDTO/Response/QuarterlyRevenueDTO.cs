namespace FitnessCal.BLL.DTO.DashboardDTO.Response
{
    public class QuarterlyRevenueDTO
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public string QuarterName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int SubscriptionCount { get; set; }
    }
}
