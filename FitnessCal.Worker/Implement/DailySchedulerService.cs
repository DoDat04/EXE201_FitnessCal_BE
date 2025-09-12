using FitnessCal.Worker.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.Worker.Implement
{
    public class DailySchedulerService : IDailySchedulerService
    {
        public TimeSpan GetDelayUntilNextRun()
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1); 
            return nextRun - now;
        }
    }
}
