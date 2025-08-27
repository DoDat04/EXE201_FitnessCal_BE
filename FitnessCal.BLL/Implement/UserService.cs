using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Implement
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    throw new KeyNotFoundException(UserMessage.USER_NOT_FOUND);
                }

                user.IsActive = 0;
                await _unitOfWork.Users.UpdateAsync(user);

                var result = await _unitOfWork.Save();

                if (result)
                {
                    _logger.LogInformation("User with ID {UserId} has been soft deleted", userId);
                }

                return result;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while soft deleting user with ID {UserId}", userId);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();

                var userDTOs = users.Select(user => new UserResponseDTO
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }).ToList();

                return userDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<bool> UnBanUserAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId);
                    throw new KeyNotFoundException(UserMessage.USER_NOT_FOUND);
                }

                user.IsActive = 1;
                await _unitOfWork.Users.UpdateAsync(user);

                var result = await _unitOfWork.Save();

                if (result)
                {
                    _logger.LogInformation("User with ID {UserId} has been unbanned", userId);
                }

                return result;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while soft deleting user with ID {UserId}", userId);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<UserStatisticsDTO> GetUserStatisticsAsync()
        {
            try
            {
                var allUsers = await _unitOfWork.Users.GetAllAsync();
                
                var currentDate = DateTime.UtcNow;
                var currentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                var lastMonth = currentMonth.AddMonths(-1);

                var totalUsers = allUsers.Count(u => u.IsActive == 1);

                        var newUsersThisMonth = allUsers.Count(u => 
            u.CreatedAt >= currentMonth && 
            u.CreatedAt < currentMonth.AddMonths(1) &&
            u.IsActive == 1);

        var newUsersLastMonth = allUsers.Count(u => 
            u.CreatedAt >= lastMonth && 
            u.CreatedAt < currentMonth &&
            u.IsActive == 1);

                var growthFromLastMonth = newUsersThisMonth - newUsersLastMonth;
                var growthPercentage = newUsersLastMonth > 0 ? 
                    ((double)growthFromLastMonth / newUsersLastMonth) * 100 : 0;

                var statistics = new UserStatisticsDTO
                {
                    TotalUsers = totalUsers,
                    NewUsersThisMonth = newUsersThisMonth,
                    NewUsersLastMonth = newUsersLastMonth,
                    GrowthFromLastMonth = growthFromLastMonth,
                    GrowthPercentage = Math.Round(growthPercentage, 1)
                };

                _logger.LogInformation("User statistics retrieved successfully. Total: {Total}, This month: {ThisMonth}, Last month: {LastMonth}", 
                    totalUsers, newUsersThisMonth, newUsersLastMonth);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user statistics");
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }
    }
}
