using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.BackgroundService.Define
{
    public interface IChangePaymentStatusGeneratorService
    {
        Task ExecuteChangePaymentStatusJob(CancellationToken stoppingToken);
    }
}
