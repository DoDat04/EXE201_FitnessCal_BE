using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IUserHealthRepository UserHealths { get; }
        IFoodRepository Foods { get; }
        IPredefinedDishRepository PredefinedDishes { get; }
        IUserMealItemRepository UserMealItems { get; }
        IUserMealLogRepository UserMealLogs { get; }
        Task<bool> Save();
    }
}
