using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Helpers
{
    public class CurrentUserIdHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserIdHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("UserId không tồn tại trong token");

            return Guid.Parse(userIdClaim.Value);
        }
    }
}
