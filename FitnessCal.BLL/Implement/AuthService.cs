using FitnessCal.BLL.Define;
using FitnessCal.BLL.Constants;
using FitnessCal.BLL.DTO.AuthDTO.Request;
using FitnessCal.BLL.DTO.AuthDTO.Response;
using FitnessCal.BLL.DTO.UserMealLogDTO.Request;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FitnessCal.BLL.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IUserMealLogService _userMealLogService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IUserMealLogService userMealLogService, IEmailService emailService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _userMealLogService = userMealLogService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_USER_NOT_FOUND);
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValidPassword)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_FAILED);
            }

            if (user.IsActive != 1)
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
                PasswordHash = HashPassword(request.Password), 
                Role = "User", 
                IsActive = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.Save();

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var createMealLogDto = new CreateUserMealLogDTO
                {
                    MealDate = today
                };

                await _userMealLogService.AutoCreateMealLogsAsync(newUser.UserId, createMealLogDto);
                _logger.LogInformation("Auto-created meal logs for new user {UserId} on {Date}", newUser.UserId, today);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-create meal logs for new user {UserId} on {Date}", newUser.UserId, DateOnly.FromDateTime(DateTime.UtcNow));
            }

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

            if (user.IsActive != 1)
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

        public async Task<LoginResponseDTO> GoogleLoginAsync(GoogleLoginRequestDTO request)
        {
            var existingUser = await _unitOfWork.Users.GetBySupabaseUserIdAsync(request.Uid);
            
            User user;
            
            if (existingUser != null)
            {
                user = existingUser;
                _logger.LogInformation("Google login for existing user: {Email}", request.Email);
            }
            else
            {
                var userByEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                
                if (userByEmail != null)
                {
                    throw new InvalidOperationException("Email đã được sử dụng để đăng ký thông thường. Vui lòng đăng nhập bằng email/password hoặc sử dụng email khác để đăng nhập Google.");
                }
                else
                {
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        PasswordHash = "", 
                        Role = "User",
                        IsActive = 1,
                        CreatedAt = request.CreatedAt,
                        SupabaseUserId = request.Uid
                    };

                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.Save();

                    try
                    {
                        var today = DateOnly.FromDateTime(DateTime.UtcNow);
                        var createMealLogDto = new CreateUserMealLogDTO
                        {
                            MealDate = today
                        };

                        await _userMealLogService.AutoCreateMealLogsAsync(user.UserId, createMealLogDto);
                        _logger.LogInformation("Auto-created meal logs for new Google user {UserId} on {Date}", user.UserId, today);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to auto-create meal logs for new Google user {UserId} on {Date}", user.UserId, DateOnly.FromDateTime(DateTime.UtcNow));
                    }

                    _logger.LogInformation("Created new user from Google login: {Email}", request.Email);
                }
            }

            if (user.IsActive != 1)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_USER_NOT_ACTIVE);
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        
        public async Task<LoginResponseDTO> DiscordLoginAsync(DiscordLoginRequestDTO request)
        {
            var existingUser = await _unitOfWork.Users.GetBySupabaseUserIdAsync(request.Uid);
            
            User user;
            
            if (existingUser != null)
            {
                user = existingUser;
                _logger.LogInformation("Discord login for existing user: {Email}", request.Email);
            }
            else
            {
                var userByEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                
                if (userByEmail != null)
                {
                    throw new InvalidOperationException("Email đã được sử dụng để đăng ký thông thường. Vui lòng đăng nhập bằng email/password hoặc sử dụng email khác để đăng nhập Discord.");
                }
                else
                {
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        PasswordHash = "", 
                        Role = "User",
                        IsActive = 1,
                        CreatedAt = request.CreatedAt,
                        SupabaseUserId = request.Uid
                    };

                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.Save();

                    try
                    {
                        var today = DateOnly.FromDateTime(DateTime.UtcNow);
                        var createMealLogDto = new CreateUserMealLogDTO
                        {
                            MealDate = today
                        };

                        await _userMealLogService.AutoCreateMealLogsAsync(user.UserId, createMealLogDto);
                        _logger.LogInformation("Auto-created meal logs for new Discord user {UserId} on {Date}", user.UserId, today);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to auto-create meal logs for new Discord user {UserId} on {Date}", user.UserId, DateOnly.FromDateTime(DateTime.UtcNow));
                    }

                    _logger.LogInformation("Created new user from Discord login: {Email}", request.Email);
                }
            }

            if (user.IsActive != 1)
            {
                throw new UnauthorizedAccessException(AuthMessage.LOGIN_USER_NOT_ACTIVE);
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
