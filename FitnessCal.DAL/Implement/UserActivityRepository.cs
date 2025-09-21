using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement;

public class UserActivityRepository : GenericRepository<UserActivity>, IUserActivityRepository
{
    public UserActivityRepository(FitnessCalContext context) : base(context)
    {
    }
}
