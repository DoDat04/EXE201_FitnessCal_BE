using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement;

public class ActivityRepository : GenericRepository<Activity>, IActivityRepository
{
    public ActivityRepository(FitnessCalContext context) : base(context)
    {
    }
}