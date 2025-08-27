using FitnessCal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.DAL.Define
{
    public interface IUserWeightLogRepository : IGenericRepository<UserWeightLog>
    {
        Task<IEnumerable<UserWeightLog>> GetUserWeightLogsByUserIdAsync(Guid userId);
        Task<IEnumerable<UserWeightLog>> GetUserWeightLogsByPeriodAsync(Guid userId, int months);
        Task<UserWeightLog?> GetByUserAndDateAsync(Guid userId, DateOnly logDate);
    }
}
