using FitnessCal.BLL.Define;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FitnessCal.Worker
{
    public class DailyMealLogWorker : BackgroundService
    {
        private readonly ILogger<DailyMealLogWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DailyMealLogWorker(IServiceProvider serviceProvider, ILogger<DailyMealLogWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyMealLogWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mealLogService = scope.ServiceProvider.GetRequiredService<IUserMealLogService>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);

                    // Lấy trực tiếp users chưa có meal log hôm nay
                    var usersWithoutLog = await userService.GetUsersWithoutMealLogAsync(today);

                    if (!usersWithoutLog.Any())
                    {
                        _logger.LogInformation("All users already have meal logs for {Date}", today);
                    }
                    else
                    {
                        _logger.LogInformation("Creating meal logs for {Count} users on {Date}", usersWithoutLog.Count(), today);

                        // Parallel xử lý để tăng tốc (giới hạn concurrency nếu cần)
                        var tasks = usersWithoutLog.Select(user =>
                            mealLogService.AutoCreateMealLogsAsync(user.UserId, new BLL.DTO.UserMealLogDTO.Request.CreateUserMealLogDTO
                            {
                                MealDate = today
                            })
                        );

                        await Task.WhenAll(tasks);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DailyMealLogWorker");
                }

                // Delay 24h
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
