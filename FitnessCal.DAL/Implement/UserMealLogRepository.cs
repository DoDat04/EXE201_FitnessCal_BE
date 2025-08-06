using FitnessCal.DAL.Context;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class UserMealLogRepository : GenericRepository<UserMealLog>, IUserMealLogRepository
    {
        public UserMealLogRepository(FitnessCalContext context) : base(context)
        {
    }
}
