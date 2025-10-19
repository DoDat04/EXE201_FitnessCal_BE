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

        public async Task<RevenueStatisticsDTO> GetRevenueStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var allSubscriptions = await _unitOfWork.UserSubscriptions.GetAllAsync(s => s.PaymentStatus == "paid" || s.PaymentStatus == "expired");

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

                // Xử lý date range
                decimal revenueInRange = 0;
                int subscriptionCountInRange = 0;
                var dailyRevenues = new List<DailyRevenueDTO>();

                if (startDate.HasValue && endDate.HasValue)
                {
                    // Đảm bảo endDate bao gồm cả ngày cuối
                    var adjustedEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    
                    var subscriptionsInRange = allSubscriptions
                        .Where(s => s.StartDate >= startDate.Value && s.StartDate <= adjustedEndDate)
                        .ToList();

                    revenueInRange = subscriptionsInRange.Sum(s => s.PriceAtPurchase);
                    subscriptionCountInRange = subscriptionsInRange.Count;

                    // Tạo dữ liệu theo ngày
                    var currentDate = startDate.Value.Date;
                    while (currentDate <= endDate.Value.Date)
                    {
                        var nextDate = currentDate.AddDays(1);
                        var daySubscriptions = subscriptionsInRange
                            .Where(s => s.StartDate >= currentDate && s.StartDate < nextDate)
                            .ToList();

                        dailyRevenues.Add(new DailyRevenueDTO
                        {
                            Date = ConvertUtcToVietnamTime(currentDate),
                            Revenue = daySubscriptions.Sum(s => s.PriceAtPurchase),
                            SubscriptionCount = daySubscriptions.Count
                        });

                        currentDate = nextDate;
                    }
                }

                // Tính toán dữ liệu theo các khoảng thời gian
                var dailyRevenuesWithData = new List<DailyRevenueDTO>();
                var monthlyRevenues = new List<MonthlyRevenueDTO>();
                var quarterlyRevenues = new List<QuarterlyRevenueDTO>();
                var yearlyRevenues = new List<YearlyRevenueDTO>();

                // Lấy tất cả ngày có doanh thu (chỉ những ngày có subscription)
                var daysWithRevenue = allSubscriptions
                    .GroupBy(s => s.StartDate.Date)
                    .Select(g => new DailyRevenueDTO
                    {
                        Date = ConvertUtcToVietnamTime(g.Key),
                        Revenue = g.Sum(s => s.PriceAtPurchase),
                        SubscriptionCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                dailyRevenuesWithData = daysWithRevenue;

                // Tính toán theo tháng
                var monthlyData = allSubscriptions
                    .GroupBy(s => new { s.StartDate.Year, s.StartDate.Month })
                    .Select(g => new MonthlyRevenueDTO
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MM/yyyy"),//số 1 là day
                        Revenue = g.Sum(s => s.PriceAtPurchase),
                        SubscriptionCount = g.Count()
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList();

                monthlyRevenues = monthlyData;

                // Tính toán theo quý
                var quarterlyData = allSubscriptions
                    .GroupBy(s => new { s.StartDate.Year, Quarter = ((s.StartDate.Month - 1) / 3) + 1 })
                    .Select(g => new QuarterlyRevenueDTO
                    {
                        Year = g.Key.Year,
                        Quarter = g.Key.Quarter,
                        QuarterName = $"Q{g.Key.Quarter}/{g.Key.Year}",
                        Revenue = g.Sum(s => s.PriceAtPurchase),
                        SubscriptionCount = g.Count()
                    })
                    .OrderBy(q => q.Year).ThenBy(q => q.Quarter)
                    .ToList();

                quarterlyRevenues = quarterlyData;

                // Tính toán theo năm
                var yearlyData = allSubscriptions
                    .GroupBy(s => s.StartDate.Year)
                    .Select(g => new YearlyRevenueDTO
                    {
                        Year = g.Key,
                        Revenue = g.Sum(s => s.PriceAtPurchase),
                        SubscriptionCount = g.Count()
                    })
                    .OrderBy(y => y.Year)
                    .ToList();

                yearlyRevenues = yearlyData;

                return new RevenueStatisticsDTO
                {
                    TotalRevenue = totalRevenue,
                    RevenueThisMonth = revenueThisMonth,
                    RevenueThisQuarter = revenueThisQuater,
                    RevenueLastMonth = revenueLastMonth,
                    RevenueGrowthPercentage = Math.Round(revenueGrowthPercentage, 1),
                    TotalRevenueYTD = totalRevenueYTD,
                    TotalRevenueYTDLastYear = totalRevenueYTDLastYear,
                    TotalRevenueYTDGrowthPercentage = Math.Round(totalRevenueYTDGrowthPercentage, 1),
                    StartDate = startDate,
                    EndDate = endDate,
                    RevenueInRange = revenueInRange,
                    SubscriptionCountInRange = subscriptionCountInRange,
                    DailyRevenues = dailyRevenues,
                    DailyRevenuesWithData = dailyRevenuesWithData,
                    MonthlyRevenues = monthlyRevenues,
                    QuarterlyRevenues = quarterlyRevenues,
                    YearlyRevenues = yearlyRevenues
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving revenue statistics");
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }

        private static DateTime ConvertUtcToVietnamTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                    return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
                }
                catch
                {
                    return utcDateTime.AddHours(7);
                }
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

        public Task<UserResponseDTO?> GetUserByIdAsync(Guid userId)
        {
            var user = _unitOfWork.Users
                .FindAsync(u => u.UserId == userId && u.Role == "User")
                .ContinueWith(t =>
                {
                    var u = t.Result;
                    if (u == null) return null;
                    return new UserResponseDTO
                    {
                        UserId = u.UserId,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt
                    };
                });
            return user;
        }

        public async Task<ConversionRateResponseDTO> GetConversionRateAsync()
        {
            try
            {
                // Lấy tất cả users và subscriptions
                var allUsers = await _unitOfWork.Users.GetAllAsync(u => u.IsActive == 1);
                var allSubscriptions = await _unitOfWork.UserSubscriptions
                    .GetAllAsync(s => s.PaymentStatus == "paid" || s.PaymentStatus == "expired");

                // Tìm tháng đầu tiên có user đăng ký
                var firstUserDate = allUsers.Min(u => u.CreatedAt);
                var lastUserDate = allUsers.Max(u => u.CreatedAt);
                
                // Tìm tháng đầu tiên có subscription
                var firstSubscriptionDate = allSubscriptions.Any() ? 
                    allSubscriptions.Min(s => s.StartDate) : DateTime.UtcNow;
                
                // Lấy khoảng thời gian từ tháng đầu tiên đến hiện tại
                var startDate = new DateTime(Math.Min(firstUserDate.Year, firstSubscriptionDate.Year), 
                    Math.Min(firstUserDate.Month, firstSubscriptionDate.Month), 1);
                var currentDate = DateTime.UtcNow;
                
                var chartData = new List<ConversionRateDataDTO>();
                var cumulativeFreeUsers = 0;
                var cumulativePremiumUsers = 0;

                // Tạo dữ liệu cho từng tháng từ tháng đầu tiên đến hiện tại
                var currentMonth = startDate;
                while (currentMonth <= currentDate)
                {
                    var monthEnd = currentMonth.AddMonths(1);
                    var monthLabel = $"Tháng {currentMonth.Month}/{currentMonth.Year}";

                    // Đếm số user free trong tháng 
                    var freeUsersInMonth = allUsers.Count(u => 
                        u.CreatedAt >= currentMonth && u.CreatedAt < monthEnd);

                    // Đếm số user mới chuyển sang premium trong tháng
                    var newPremiumUsersInMonth = allSubscriptions.Count(s => 
                        s.StartDate >= currentMonth && s.StartDate < monthEnd);

                    // Tính conversion rate
                    var conversionRate = freeUsersInMonth > 0 ? 
                        (double)newPremiumUsersInMonth / freeUsersInMonth * 100 : 0;

                    cumulativeFreeUsers += freeUsersInMonth;
                    cumulativePremiumUsers += newPremiumUsersInMonth;

                    chartData.Add(new ConversionRateDataDTO
                    {
                        MonthLabel = monthLabel,
                        TotalFreeUsers = freeUsersInMonth,
                        NewPremiumUsers = newPremiumUsersInMonth,
                        ConversionRate = Math.Round(conversionRate, 2),
                        CumulativeFreeUsers = cumulativeFreeUsers,
                        CumulativePremiumUsers = cumulativePremiumUsers
                    });

                    currentMonth = currentMonth.AddMonths(1);
                }

                _logger.LogInformation("Conversion rate data retrieved successfully. Chart data points: {Count}", 
                    chartData.Count);

                return new ConversionRateResponseDTO
                {
                    ChartData = chartData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving conversion rate data");
                throw new Exception(ResponseCodes.Messages.DATABASE_ERROR);
            }
        }
    }
}
