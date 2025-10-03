using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class PackageFeatureRepository : GenericRepository<PackageFeature>, IPackageFeatureRepository
    {
        public PackageFeatureRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}


