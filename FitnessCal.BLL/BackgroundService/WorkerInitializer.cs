using FitnessCal.BLL.BackgroundService.Define;
using FitnessCal.BLL.BackgroundService.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.BackgroundService
{
    public static class WorkerInitializer
    {
        public static void StartAll(IServiceProvider serviceProvider, ILogger logger = null)
        {
            try
            {
                var workerList = new object[]
                {
                    serviceProvider.GetRequiredService<DailyMealLogWorker>(),
                    serviceProvider.GetRequiredService<MealNotificationWorker>(),
                    serviceProvider.GetRequiredService<ChangePaymentStatusWorker>(),
                    serviceProvider.GetRequiredService<DeleteFailedPaymentStatusWorker>()
                };

                foreach (var workerObj in workerList)
                {
                    if (workerObj is IWorkerBase worker)
                    {
                        worker.Start();
                        logger?.LogInformation($"{worker.GetType().Name} started successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error starting workers");
                throw;
            }
        }
    }
}
