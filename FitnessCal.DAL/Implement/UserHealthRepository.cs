using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class UserHealthRepository : GenericRepository<UserHealth>, IUserHealthRepository
    {
        public UserHealthRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}
