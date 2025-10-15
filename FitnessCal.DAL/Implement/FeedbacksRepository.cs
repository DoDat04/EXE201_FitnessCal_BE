using FitnessCal.DAL.Define;
using FitnessCal.Domain;

namespace FitnessCal.DAL.Implement
{
    public class FeedbacksRepository : GenericRepository<Feedbacks>, IFeedbacksRepository
    {
        public FeedbacksRepository(FitnessCalContext context) : base(context)
        {
        }
    }
}
