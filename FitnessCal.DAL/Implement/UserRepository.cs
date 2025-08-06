using FitnessCal.DAL.Context;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.DAL.Implement
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly FitnessCalContext _fitnessCalContext;

        public UserRepository(FitnessCalContext context) : base(context)
        {
            _fitnessCalContext = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _fitnessCalContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
