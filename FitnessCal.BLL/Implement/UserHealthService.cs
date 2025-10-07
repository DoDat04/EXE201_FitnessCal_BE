using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserHealthDTO.Response;
using FitnessCal.DAL.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Implement
{
    public class UserHealthService : IUserHealthService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserHealthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<HealthUserInfoDTO?> GetHealthUserInfoAsync(Guid userId)
        {
            var userHealth = await _unitOfWork.UserHealths.GetByIdAsync(
                    uh => uh.UserId == userId,
                    uh => uh.User,
                    uh => uh.User.UserSubscriptions
                );
            if (userHealth == null)
            {
                return null;
            }

            var PaymentStatus = userHealth.User.UserSubscriptions.FirstOrDefault(sub => sub.PaymentStatus == "paid") != null ? "Premium" : "Free";
            return new HealthUserInfoDTO
            {
                UserId = userHealth.UserId,
                FullName = (userHealth.User != null
                    ? $"{userHealth.User.FirstName} {userHealth.User.LastName}".Trim()
                    : string.Empty),
                DateOfBirth = userHealth.DateOfBirth.HasValue
                    ? userHealth.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                    : DateTime.MinValue,
                Gender = userHealth.Gender ?? string.Empty,
                Height = userHealth.HeightCm,
                Weight = userHealth.WeightKg,
                TargetWeight = userHealth.TargetWeightKg,
                ActivityLevel = userHealth.ActivityLevel ?? string.Empty,
                FitnessGoal = userHealth.Goal ?? string.Empty,
                DietType = userHealth.DietType ?? string.Empty,
                IntensityLevel = userHealth.IntensityLevel ?? string.Empty,
                EstimateGoalAt = userHealth.EstimatedGoalDate.HasValue
                    ? userHealth.EstimatedGoalDate.Value.ToDateTime(TimeOnly.MinValue)
                    : DateTime.MinValue,
                DailyCalories = (float)(userHealth.DailyCalories ?? 0),
                PaymentStatus = PaymentStatus
            };
        }
    }
}
