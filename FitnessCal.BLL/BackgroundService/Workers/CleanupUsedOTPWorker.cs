using FitnessCal.BLL.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.BackgroundService.Workers
{
    public class CleanupUsedOTPWorker
    {
        private readonly ILogger<CleanupUsedOTPWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _runInterval;

        public CleanupUsedOTPWorker(ILogger<CleanupUsedOTPWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _runInterval = TimeSpan.FromHours(1); // khoảng thời gian chạy lại
        }

        public async Task RunAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("CleanupUsedOTPWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var otpService = scope.ServiceProvider.GetRequiredService<IOTPService>();

                    await otpService.CleanupUsedOTPsAsync();
                    await otpService.CleanupExpiredOTPsAsync();

                    _logger.LogInformation("Used & expired OTPs cleaned up successfully at {time}.", DateTimeOffset.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up OTPs at {time}", DateTimeOffset.UtcNow);
                }

                await Task.Delay(_runInterval, stoppingToken);
            }
        }
    }
}