using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.UserMealLogDTO.Request;
using FitnessCal.Worker.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.Worker.Implement
{
    public class DailyMealLogGeneratorService : IDailyMealLogGeneratorService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyMealLogGeneratorService> _logger;

        public DailyMealLogGeneratorService(IServiceScopeFactory scopeFactory, ILogger<DailyMealLogGeneratorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task GenerateDailyMealLogsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var todayStr = today.ToString("yyyy/MM/dd");

            var usersWithoutLog = await userService.GetUsersWithoutMealLogAsync(today);

            if (!usersWithoutLog.Any())
            {
                _logger.LogInformation("ℹ️ All users already have meal logs for {Date}", todayStr);
                return;
            }

            _logger.LogInformation("⚡ Creating meal logs for {Count} users on {Date}", usersWithoutLog.Count(), todayStr);

            var tasks = usersWithoutLog.Select(async user =>
            {
                using var innerScope = _scopeFactory.CreateScope();
                var mealLogService = innerScope.ServiceProvider.GetRequiredService<IUserMealLogService>();

                try
                {
                    _logger.LogInformation("➡️ Creating meal log for UserId={UserId}, Email={Email}, Date={Date}",
                        user.UserId, user.Email, todayStr);

                    await mealLogService.AutoCreateMealLogsAsync(user.UserId, new CreateUserMealLogDTO
                    {
                        MealDate = today
                    });

                    _logger.LogInformation("✅ Meal log created for UserId={UserId}", user.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to create meal log for UserId={UserId}", user.UserId);
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
