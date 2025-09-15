using FitnessCal.BLL.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class CleanupUsedOTPWorker : BackgroundService
{
    private readonly ILogger<CleanupUsedOTPWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _runInterval = TimeSpan.FromHours(1);

    public CleanupUsedOTPWorker(ILogger<CleanupUsedOTPWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

                _logger.LogInformation("Used & expired OTPs cleaned up successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up OTPs");
            }

            await Task.Delay(_runInterval, stoppingToken);
        }
    }
}
