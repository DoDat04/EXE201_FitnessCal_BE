using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class PredefinedDishRepository : GenericRepository<PredefinedDish>, IPredefinedDishRepository
    {
        public PredefinedDishRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}
