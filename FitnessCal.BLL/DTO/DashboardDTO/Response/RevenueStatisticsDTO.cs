namespace FitnessCal.BLL.DTO.DashboardDTO.Response
{
    public class RevenueStatisticsDTO
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal RevenueThisQuarter { get; set; }
        public double RevenueGrowthPercentage { get; set; }
        
        // YTD YoY
        public decimal TotalRevenueYTD { get; set; }
        public decimal TotalRevenueYTDLastYear { get; set; }
        public double TotalRevenueYTDGrowthPercentage { get; set; }
        
        // Date range data
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal RevenueInRange { get; set; }
        public int SubscriptionCountInRange { get; set; }
        public List<DailyRevenueDTO> DailyRevenues { get; set; } = new List<DailyRevenueDTO>();
    }
}


