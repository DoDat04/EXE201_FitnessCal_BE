namespace FitnessCal.BLL.DTO.DashboardDTO.Response
{
    public class RevenueStatisticsDTO
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public double RevenueGrowthPercentage { get; set; }
    }
}


