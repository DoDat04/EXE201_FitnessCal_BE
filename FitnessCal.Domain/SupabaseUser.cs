using Postgrest.Attributes;
using Postgrest.Models;

namespace FitnessCal.Domain
{
    [Table("users")]
    public class SupabaseUser : BaseModel
    {
        [PrimaryKey("userid", false)]
        public Guid UserId { get; set; }

        [Column("firstname")]
        public string? FirstName { get; set; }

        [Column("lastname")]
        public string? LastName { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("passwordhash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("role")]
        public string Role { get; set; } = "User";

        [Column("isactive")]
        public short IsActive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("supabase_user_id")]
        public string? SupabaseUserId { get; set; }
    }
}
