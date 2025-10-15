using FitnessCal.BLL.BackgroundService.Define;
using FitnessCal.BLL.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.BackgroundService.Workers
{
    public class ChangePaymentStatusWorker : IWorkerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChangePaymentStatusWorker> _logger;
        private readonly TimeSpan _runInterval;
        private CancellationTokenSource _cts;
        private Task? _executingTask;

        public ChangePaymentStatusWorker(
            IServiceProvider serviceProvider,
            ILogger<ChangePaymentStatusWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cts = new CancellationTokenSource();
            _runInterval = TimeSpan.FromHours(1); 
        }

        public void Start()
        {
            _logger.LogInformation("ChangePaymentStatusWorker started.");
            _executingTask = Task.Run(() => RunAsync(_cts.Token));
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping ChangePaymentStatusWorker...");
            _cts.Cancel();

            if (_executingTask != null)
            {
                try
                {
                    await _executingTask;
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                try
                {
                    await subscriptionService.CheckAndUpdateExpiredSubscriptionsAsync(cancellationToken);
                    _logger.LogInformation("Checked subscriptions at {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while updating subscription statuses.");
                }

                try
                {
                    await Task.Delay(_runInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break; 
                }
            }
        }
    }
}
