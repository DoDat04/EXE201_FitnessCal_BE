using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class UserCapturedFoodRepository : GenericRepository<UserCapturedFood>, IUserCapturedFoodRepository
    {
        public UserCapturedFoodRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}
