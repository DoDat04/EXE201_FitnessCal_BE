using FitnessCal.Domain;

namespace FitnessCal.DAL.Define
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetBySupabaseUserIdAsync(string supabaseUserId);
    }
}
