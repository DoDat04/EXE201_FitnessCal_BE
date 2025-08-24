namespace FitnessCal.BLL.DTO.CommonDTO
{
    public class UserStatisticsDTO
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersLastMonth { get; set; }
        public int GrowthFromLastMonth { get; set; }
        public double GrowthPercentage { get; set; }
    }

    public class DashboardResponseDTO
    {
        public UserStatisticsDTO UserStatistics { get; set; } = new UserStatisticsDTO();
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
