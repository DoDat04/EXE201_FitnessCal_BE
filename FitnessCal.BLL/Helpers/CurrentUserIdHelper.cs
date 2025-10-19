using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FitnessCal.BLL.Helpers
{
    public class CurrentUserIdHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserIdHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected internal Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim == null
                ? throw new UnauthorizedAccessException("UserId không tồn tại trong token")
                : Guid.Parse(userIdClaim.Value);
        }
    }
}
