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
using Microsoft.Extensions.Caching.Memory;

namespace FitnessCal.BLL.Implement
{
    internal class RegistrationInfo
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    internal class RegistrationTempStore
    {
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        public void Set(string email, string token, RegistrationInfo info, TimeSpan ttl)
        {
            Cache.Set(GetKey(email, token), info, ttl);
        }

        public RegistrationInfo? Get(string email, string token)
        {
            Cache.TryGetValue(GetKey(email, token), out RegistrationInfo? info);
            return info;
        }

        public void Remove(string email, string token)
        {
            Cache.Remove(GetKey(email, token));
        }

        private static string GetKey(string email, string token) => $"reg:{email}:{token}";
    }
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IUserMealLogService _userMealLogService;
        private readonly IEmailService _emailService;
        private readonly IOTPService _otpService;
        private readonly ILogger<AuthService> _logger;
        private static readonly RegistrationTempStore RegistrationTempStore = new RegistrationTempStore();

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IUserMealLogService userMealLogService, IEmailService emailService, IOTPService otpService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _userMealLogService = userMealLogService;
            _emailService = emailService;
            _otpService = otpService;
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

            var registrationToken = Guid.NewGuid().ToString("N");
            RegistrationTempStore.Set(request.Email, registrationToken, new RegistrationInfo
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = HashPassword(request.Password)
            }, TimeSpan.FromMinutes(15));

            var otpResult = await _otpService.SendOTPAsync(request.Email, "REGISTER");
            if (!otpResult.Success)
            {
                throw new InvalidOperationException(otpResult.Message);
            }

            _logger.LogInformation("OTP sent for registration: {Email}", request.Email);

            return new RegisterResponseDTO
            {
                Email = request.Email,
                Message = "Vui lòng kiểm tra email và nhập mã xác thực để hoàn tất đăng ký.",
                RegistrationToken = registrationToken
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

        public async Task<bool> ActivateUserAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy tài khoản với email này.");
            }

            if (user.IsActive == 1)
            {
                throw new InvalidOperationException("Tài khoản đã được kích hoạt.");
            }

            // Kích hoạt tài khoản
            user.IsActive = 1;
            await _unitOfWork.Save();

            // Tạo meal logs cho user đã kích hoạt
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var createMealLogDto = new CreateUserMealLogDTO
                {
                    MealDate = today
                };

                await _userMealLogService.AutoCreateMealLogsAsync(user.UserId, createMealLogDto);
                _logger.LogInformation("Auto-created meal logs for activated user {UserId} on {Date}", user.UserId, today);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-create meal logs for activated user {UserId} on {Date}", user.UserId, DateOnly.FromDateTime(DateTime.UtcNow));
            }

            _logger.LogInformation("User activated successfully: {Email}", email);
            return true;
        }

        public async Task<bool> CreateUserAfterOTPVerificationAsync(string email, string otpCode, string registrationToken)
        {
            var otpResponse = await _otpService.VerifyOTPAsync(email, otpCode, "REGISTER");
            if (!otpResponse.Success)
            {
                throw new InvalidOperationException(otpResponse.Message);
            }

            var registrationInfo = RegistrationTempStore.Get(email, registrationToken);
            if (registrationInfo == null)
            {
                throw new InvalidOperationException("Thông tin đăng ký đã hết hạn hoặc không hợp lệ.");
            }

            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Tài khoản đã tồn tại.");
            }

            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = email,
                FirstName = registrationInfo.FirstName,
                LastName = registrationInfo.LastName,
                PasswordHash = registrationInfo.PasswordHash,
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

            _logger.LogInformation("User created successfully after OTP verification: {Email}", email);

            RegistrationTempStore.Remove(email, registrationToken);
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("Email không tồn tại trong hệ thống.");
            }

            if (user.IsActive != 1)
            {
                throw new InvalidOperationException("Tài khoản chưa được kích hoạt.");
            }

            var otpResult = await _otpService.SendOTPAsync(email, "RESET_PASSWORD");
            if (!otpResult.Success)
            {
                throw new InvalidOperationException(otpResult.Message);
            }

            _logger.LogInformation("OTP sent for password reset: {Email}", email);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otpCode, string newPassword)
        {
            var otpResponse = await _otpService.VerifyOTPAsync(email, otpCode, "RESET_PASSWORD");
            if (!otpResponse.Success)
            {
                throw new InvalidOperationException(otpResponse.Message);
            }

            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("Email không tồn tại trong hệ thống.");
            }

            if (user.IsActive != 1)
            {
                throw new InvalidOperationException("Tài khoản chưa được kích hoạt.");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _unitOfWork.Save();

            _logger.LogInformation("Password reset successfully for user: {Email}", email);
            return true;
        }
    }
}
