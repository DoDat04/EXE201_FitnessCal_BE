using FitnessCal.BLL.BackgroundService.Define;
using FitnessCal.BLL.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.BackgroundService.Implement
{
    public class ChangePaymentStatusGeneratorService : IChangePaymentStatusGeneratorService
    {
        private readonly ILogger<ChangePaymentStatusGeneratorService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ChangePaymentStatusGeneratorService(ILogger<ChangePaymentStatusGeneratorService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task ExecuteChangePaymentStatusJob(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SubscriptionStatusBackgroundWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                try
                {
                    await service.CheckAndUpdateExpiredSubscriptionsAsync(stoppingToken);
                    _logger.LogInformation("Checked subscriptions at: {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while updating subscription statuses.");
                }

                // chạy mỗi 1 giờ (có thể chỉnh)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
