using FitnessCal.BLL.Define;
using FitnessCal.BLL.Constants;
using FitnessCal.BLL.DTO.AuthDTO.Request;
using FitnessCal.BLL.DTO.AuthDTO.Response;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_USER_NOT_FOUND);
            }

            if (user.PasswordHash != request.Password)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_FAILED);
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_USER_NOT_ACTIVE);
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request)
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(AuthMessage.REGISTER_EMAIL_EXISTS);
            }

            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = request.Password, 
                Role = "User", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.Save();

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return new RegisterResponseDTO
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName
            };
        }

        public Task<bool> LogoutAsync(string refreshToken)
        {
            var claimsPrincipal = _jwtService.ValidateRefreshToken(refreshToken);

            if (claimsPrincipal == null)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGOUT_INVALID_TOKEN);
            }

            _logger.LogInformation("User logged out successfully");

            return Task.FromResult(true);
        }

        public async Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken)
        {
            var claimsPrincipal = _jwtService.ValidateRefreshToken(refreshToken);
            
            if (claimsPrincipal == null)
            {
                throw new UnauthorizedAccessException(AuthMessage.REFRESH_INVALID_TOKEN);
            }

            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                throw new UnauthorizedAccessException(AuthMessage.REFRESH_USER_NOT_FOUND);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userGuid);
            
            if (user == null)
            {
                throw new UnauthorizedAccessException(AuthMessage.REFRESH_USER_NOT_FOUND);
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_USER_NOT_ACTIVE);
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken(user);

            _logger.LogInformation("Token refreshed successfully for user: {Email}", user.Email);

            return new LoginResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
