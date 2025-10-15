using FitnessCal.BLL.BackgroundService.Define;
using FitnessCal.BLL.Define;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.BackgroundService.Workers
{
    public class DeleteFailedPaymentStatusWorker : IWorkerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChangePaymentStatusWorker> _logger;
        private readonly TimeSpan _runInterval;
        private CancellationTokenSource _cts;
        private Task? _executingTask;
        public DeleteFailedPaymentStatusWorker(
            IServiceProvider serviceProvider,
            ILogger<ChangePaymentStatusWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cts = new CancellationTokenSource();
            _runInterval = TimeSpan.FromHours(24);
        }
        public void Start()
        {
            _logger.LogInformation("DeleteFailedPaymentStatusWorker is starting.");
            _executingTask = Task.Run(() => RunAsync(_cts.Token));
        }
        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping DeleteFailedPaymentStatusWorker...");
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
            _logger.LogInformation("DeleteFailedPaymentStatusWorker is running.");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var paymentService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                    await paymentService.DeleteFailedPaymentsAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while deleting failed payment statuses.");
                }
                await Task.Delay(_runInterval, cancellationToken);
            }
            _logger.LogInformation("DeleteFailedPaymentStatusWorker is stopping.");
        }
    }
}
