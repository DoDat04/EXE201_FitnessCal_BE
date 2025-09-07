namespace FitnessCal.BLL.DTO.CommonDTO
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

    public class DashboardResponseDTO
    {
        public UserStatisticsDTO UserStatistics { get; set; } = new UserStatisticsDTO();
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
