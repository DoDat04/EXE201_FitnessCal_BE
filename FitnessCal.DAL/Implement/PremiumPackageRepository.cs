using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class PremiumPackageRepository : GenericRepository<PremiumPackage>, IPremiumPackageRepository
    {
        public PremiumPackageRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}
