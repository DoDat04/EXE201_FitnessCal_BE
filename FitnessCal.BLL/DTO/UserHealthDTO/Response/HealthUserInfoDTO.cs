using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserHealthDTO.Response
{
    public class HealthUserInfoDTO
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? TargetWeight { get; set; }
        public string ActivityLevel { get; set; } = string.Empty;
        public string FitnessGoal { get; set; } = string.Empty;
        public string DietType { get; set; }
        public string IntensityLevel { get; set; } = string.Empty;
        public DateTime EstimateGoalAt { get; set; }
        public float DailyCalories { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
