using ClosedXML.Excel;
using FitnessCal.BLL.Constants;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.DashboardDTO.Response;
using FitnessCal.BLL.DTO.UserDTO.Response;
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
                var users = await _unitOfWork.Users.GetAllAsync(u => u.Role == "User");

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
                var allSubscriptions = await _unitOfWork.UserSubscriptions.GetAllAsync(s => s.PaymentStatus == "paid");
                
                var currentDate = DateTime.UtcNow;
                var currentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                var lastMonth = currentMonth.AddMonths(-1);

                // Total Users Statistics
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

                var premiumUsers = allSubscriptions.Select(s => s.UserId).Distinct().Count();
                var newPremiumUsersThisMonth = allSubscriptions.Count(s => 
                    s.StartDate >= currentMonth && 
                    s.StartDate < currentMonth.AddMonths(1));
                var newPremiumUsersLastMonth = allSubscriptions.Count(s => 
                    s.StartDate >= lastMonth && 
                    s.StartDate < currentMonth);
                var premiumGrowthFromLastMonth = newPremiumUsersThisMonth - newPremiumUsersLastMonth;
                var premiumGrowthPercentage = newPremiumUsersLastMonth > 0 ? 
                    ((double)premiumGrowthFromLastMonth / newPremiumUsersLastMonth) * 100 : 0;

                var statistics = new UserStatisticsDTO
                {
                    // Total Users
                    TotalUsers = totalUsers,
                    NewUsersThisMonth = newUsersThisMonth,
                    NewUsersLastMonth = newUsersLastMonth,
                    GrowthFromLastMonth = growthFromLastMonth,
                    GrowthPercentage = Math.Round(growthPercentage, 1),
                    
                    // Premium Users
                    PremiumUsers = premiumUsers,
                    NewPremiumUsersThisMonth = newPremiumUsersThisMonth,
                    NewPremiumUsersLastMonth = newPremiumUsersLastMonth,
                    PremiumGrowthFromLastMonth = premiumGrowthFromLastMonth,
                    PremiumGrowthPercentage = Math.Round(premiumGrowthPercentage, 1)
                };

                _logger.LogInformation("User statistics retrieved successfully. Total: {Total}, Premium: {Premium}, This month: {ThisMonth}, Last month: {LastMonth}", 
                    totalUsers, premiumUsers, newUsersThisMonth, newUsersLastMonth);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user statistics");
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<IEnumerable<UserResponseDTO>> GetUsersWithoutMealLogAsync(DateOnly today)
        {
            try
            {
                // Lấy tất cả users active
                var allUsers = await _unitOfWork.Users.GetAllAsync(u => u.IsActive == 1 && u.Role == "User");

                // Lấy tất cả meal logs cho ngày hôm nay
                var mealLogsToday = await _unitOfWork.UserMealLogs
                    .GetAllAsync(l => l.MealDate == today);

                var userIdsWithLogs = mealLogsToday.Select(l => l.UserId).ToHashSet();

                // Chỉ lấy users chưa có meal log
                var usersWithoutLog = allUsers
                    .Where(u => !userIdsWithLogs.Contains(u.UserId))
                    .Select(u => new UserResponseDTO
                    {
                        UserId = u.UserId,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt
                    })
                    .ToList();

                _logger.LogInformation("Found {Count} users without meal log for {Date}", usersWithoutLog.Count, today);

                return usersWithoutLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users without meal log for {Date}", today);
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<RevenueStatisticsDTO> GetRevenueStatisticsAsync()
        {
            try
            {
                var allSubscriptions = await _unitOfWork.UserSubscriptions.GetAllAsync(s => s.PaymentStatus == "paid");

                var now = DateTime.UtcNow;
                var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfThisMonth.AddMonths(-1);
                var startOfNextMonth = startOfThisMonth.AddMonths(1);
                var startOfThisYear = new DateTime(now.Year, 1, 1);
                var startOfLastYear = new DateTime(now.Year - 1, 1, 1);
                var startOfThisDayLastYear = new DateTime(now.Year - 1, now.Month, now.Day);

                // Doanh thu tổng (paid)
                var totalRevenue = allSubscriptions.Sum(s => s.PriceAtPurchase);

                // Doanh thu theo tháng (sử dụng PriceAtPurchase)
                var revenueThisMonth = allSubscriptions
                    .Where(s => s.StartDate >= startOfThisMonth && s.StartDate < startOfNextMonth)
                    .Sum(s => s.PriceAtPurchase);

                var revenueLastMonth = allSubscriptions
                    .Where(s => s.StartDate >= startOfLastMonth && s.StartDate < startOfThisMonth)
                    .Sum(s => s.PriceAtPurchase);

                var revenueThisQuater = allSubscriptions
                    .Where(s => s.StartDate >= new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1) && s.StartDate < startOfNextMonth)
                    .Sum(s => s.PriceAtPurchase);

                var revenueGrowthPercentage = revenueLastMonth > 0
                    ? (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100
                    : 0;

                // YTD YoY
                var totalRevenueYTD = allSubscriptions
                    .Where(s => s.StartDate >= startOfThisYear && s.StartDate <= now)
                    .Sum(s => s.PriceAtPurchase);

                var totalRevenueYTDLastYear = allSubscriptions
                    .Where(s => s.StartDate >= startOfLastYear && s.StartDate < startOfThisYear)
                    .Where(s => s.StartDate.Month < now.Month || (s.StartDate.Month == now.Month && s.StartDate.Day <= now.Day))
                    .Sum(s => s.PriceAtPurchase);

                var totalRevenueYTDGrowthPercentage = totalRevenueYTDLastYear > 0
                    ? (double)((totalRevenueYTD - totalRevenueYTDLastYear) / totalRevenueYTDLastYear) * 100
                    : 0;

                return new RevenueStatisticsDTO
                {
                    TotalRevenue = totalRevenue,
                    RevenueThisMonth = revenueThisMonth,
                    RevenueThisQuarter = revenueThisQuater,
                    RevenueLastMonth = revenueLastMonth,
                    RevenueGrowthPercentage = Math.Round(revenueGrowthPercentage, 1),
                    TotalRevenueYTD = totalRevenueYTD,
                    TotalRevenueYTDLastYear = totalRevenueYTDLastYear,
                    TotalRevenueYTDGrowthPercentage = Math.Round(totalRevenueYTDGrowthPercentage, 1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving revenue statistics");
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        public async Task<byte[]> ExportUsersPremiumToExcelAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync(u => u.IsActive == 1 && u.Role == "User");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("All Users");

                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "First Name";
                worksheet.Cell(1, 3).Value = "Last Name";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Created Date";

                // Ghi dữ liệu
                int row = 2;
                int index = 1;
                foreach (var user in users)
                {
                    worksheet.Cell(row, 1).Value = index++;
                    worksheet.Cell(row, 2).Value = user.FirstName;
                    worksheet.Cell(row, 3).Value = user.LastName;
                    worksheet.Cell(row, 4).Value = user.Email;
                    worksheet.Cell(row, 5).Value = user.CreatedAt;
                    row++;
                }

                // Trả về file Excel dạng byte[]
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

    }
}
