using FitnessCal.Domain;
using System.Security.Claims;

namespace FitnessCal.BLL.Define
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
        ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
        string? GetUserIdFromRefreshToken(string refreshToken);
    }
}
