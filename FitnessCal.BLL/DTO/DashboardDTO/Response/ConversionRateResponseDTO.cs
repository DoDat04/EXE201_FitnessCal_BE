namespace FitnessCal.BLL.DTO.DashboardDTO.Response
{
    public class ConversionRateResponseDTO
    {
        public List<ConversionRateDataDTO> ChartData { get; set; } = new List<ConversionRateDataDTO>();
    }

    public class ConversionRateDataDTO
    {
        public string MonthLabel { get; set; } = string.Empty; 
        public int TotalFreeUsers { get; set; } 
        public int NewPremiumUsers { get; set; } 
        public double ConversionRate { get; set; } 
        public int CumulativeFreeUsers { get; set; } 
        public int CumulativePremiumUsers { get; set; } 
    }
}
