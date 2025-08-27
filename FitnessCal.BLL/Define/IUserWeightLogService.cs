using FitnessCal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Define
{
    public interface IUserWeightLogService
    {
        Task<IEnumerable<UserWeightLog>> GetUserWeightLogsByPeriodAsync(Guid userId, int months);

        Task<IEnumerable<UserWeightLog>> GetAllUserWeightLogsAsync(Guid userId);

        Task<UserWeightLog> AddWeightLogAsync(Guid userId, decimal weightKg, DateOnly logDate);
    }
}
