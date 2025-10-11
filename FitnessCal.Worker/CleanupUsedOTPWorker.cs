using FitnessCal.BLL.Define;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessCal.Worker
{
    public class CleanupUsedOTPWorker
    {
        private readonly ILogger<CleanupUsedOTPWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public CleanupUsedOTPWorker(ILogger<CleanupUsedOTPWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // ⚙️ Chạy mỗi giờ để cleanup OTP đã sử dụng và hết hạn
        [Function("CleanupUsedOTPWorker")]
        public async Task RunAsync(
            [TimerTrigger("0 0 * * * *")] TimerInfo timer,
            FunctionContext context,
            CancellationToken cancellationToken)
        {
            var functionName = context.FunctionDefinition.Name;
            _logger.LogInformation("🚀 {FunctionName} triggered at: {time}", functionName, DateTimeOffset.UtcNow);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var otpService = scope.ServiceProvider.GetRequiredService<IOTPService>();

                await otpService.CleanupUsedOTPsAsync();
                await otpService.CleanupExpiredOTPsAsync();

                _logger.LogInformation("✅ Used & expired OTPs cleaned up successfully at {time}.", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cleaning up OTPs at {time}", DateTimeOffset.UtcNow);
            }
        }
    }
}
