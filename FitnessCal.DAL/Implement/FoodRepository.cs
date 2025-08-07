using FitnessCal.DAL.Define;
using FitnessCal.Domain;


namespace FitnessCal.DAL.Implement
{
    public class FoodRepository : GenericRepository<Food>, IFoodRepository
    {
        public FoodRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}
