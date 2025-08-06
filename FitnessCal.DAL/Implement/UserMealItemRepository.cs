using FitnessCal.DAL.Context;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class UserMealItemRepository : GenericRepository<UserMealItem>, IUserMealItemRepository
    {
        public UserMealItemRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}
