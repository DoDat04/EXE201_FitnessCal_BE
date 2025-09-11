namespace FitnessCal.BLL.DTO.DashboardDTO.Response
{
    public class UserStatisticsDTO
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersLastMonth { get; set; }
        public int GrowthFromLastMonth { get; set; }
        public double GrowthPercentage { get; set; }

        public int PremiumUsers { get; set; }
        public int NewPremiumUsersThisMonth { get; set; }
        public int NewPremiumUsersLastMonth { get; set; }
        public int PremiumGrowthFromLastMonth { get; set; }
        public double PremiumGrowthPercentage { get; set; }
    }
}


