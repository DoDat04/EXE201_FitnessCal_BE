using FitnessCal.BLL.Define;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class UserWeightLogService : IUserWeightLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserWeightLogService> _logger;

        public UserWeightLogService(IUnitOfWork unitOfWork, ILogger<UserWeightLogService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserWeightLog> AddWeightLogAsync(Guid userId, decimal weightKg, DateOnly logDate)
        {
            try
            {
                // Upsert theo user + ngày
                var existing = await _unitOfWork.UserWeightLogs.GetByUserAndDateAsync(userId, logDate);
                if (existing != null)
                {
                    existing.WeightKg = weightKg;
                    await _unitOfWork.UserWeightLogs.UpdateAsync(existing);
                    await _unitOfWork.Save();

                    _logger.LogInformation("Updated weight log for user {UserId}: {Weight}kg on {Date}", 
                        userId, weightKg, logDate);
                    return existing;
                }

                var weightLog = new UserWeightLog
                {
                    UserId = userId,
                    WeightKg = weightKg,
                    LogDate = logDate
                };

                await _unitOfWork.UserWeightLogs.AddAsync(weightLog);
                await _unitOfWork.Save();

                _logger.LogInformation("Added weight log for user {UserId}: {Weight}kg on {Date}", 
                    userId, weightKg, logDate);

                return weightLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert weight log for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserWeightLog>> GetAllUserWeightLogsAsync(Guid userId)
        {
            try
            {
                var weightLogs = await _unitOfWork.UserWeightLogs.GetUserWeightLogsByUserIdAsync(userId);
                
                _logger.LogInformation("Retrieved {Count} weight logs for user {UserId}", 
                    weightLogs.Count(), userId);

                return weightLogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all weight logs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserWeightLog>> GetUserWeightLogsByPeriodAsync(Guid userId, int months)
        {
            try
            {
                var weightLogs = await _unitOfWork.UserWeightLogs.GetUserWeightLogsByPeriodAsync(userId, months);
                
                _logger.LogInformation("Retrieved {Count} weight logs for user {UserId} in last {Months} months", 
                    weightLogs.Count(), userId, months);

                return weightLogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get weight logs for user {UserId} in last {Months} months", 
                    userId, months);
                throw;
            }
        }
    }
}
