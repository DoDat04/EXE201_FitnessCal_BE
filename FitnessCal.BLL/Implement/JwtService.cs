using FitnessCal.BLL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FitnessCal.BLL.DTO.CommonDTO;

namespace FitnessCal.BLL.Implement
{
    public class JwtService : IJwtService
    {
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings;
            _logger = logger;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

            return GenerateToken(
                claims,
                _jwtSettings.Value.AccessSecretKey,
                _jwtSettings.Value.AccessExpiration
            );
        }

        public string GenerateRefreshToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

            return GenerateToken(
                claims,
                _jwtSettings.Value.RefreshSecretKey,
                _jwtSettings.Value.RefreshExpiration
            );
        }

        private string GenerateToken(List<Claim> claims, string secretKey, int expirationMinutes)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Value.Issuer,
                audience: _jwtSettings.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Value.RefreshSecretKey);

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtSettings.Value.Issuer,
                    ValidAudience = _jwtSettings.Value.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return claimsPrincipal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("Refresh token expired: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning("Invalid refresh token signature: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                _logger.LogWarning("Invalid refresh token issuer: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                _logger.LogWarning("Invalid refresh token audience: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating refresh token");
                return null;
            }
        }

        public string? GetUserIdFromRefreshToken(string refreshToken)
        {
            var claimsPrincipal = ValidateRefreshToken(refreshToken);
            return claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
