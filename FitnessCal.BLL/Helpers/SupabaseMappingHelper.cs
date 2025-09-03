using FitnessCal.Domain;

namespace FitnessCal.BLL.Helpers
{
    public static class SupabaseMappingHelper
    {
        public static SupabaseUser ToSupabaseUser(this User user)
        {
            return new SupabaseUser
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                SupabaseUserId = user.SupabaseUserId
            };
        }

        public static User ToDomainUser(this SupabaseUser supabaseUser)
        {
            return new User
            {
                UserId = supabaseUser.UserId,
                FirstName = supabaseUser.FirstName,
                LastName = supabaseUser.LastName,
                Email = supabaseUser.Email,
                PasswordHash = supabaseUser.PasswordHash,
                Role = supabaseUser.Role,
                IsActive = supabaseUser.IsActive,
                CreatedAt = supabaseUser.CreatedAt,
                SupabaseUserId = supabaseUser.SupabaseUserId
            };
        }
    }
}
