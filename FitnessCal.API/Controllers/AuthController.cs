using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.AuthDTO.Request;
using FitnessCal.BLL.DTO.AuthDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using FitnessCal.Domain;

namespace FitnessCal.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IOTPService _otpService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IOTPService otpService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _otpService = otpService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<LoginResponseDTO>
                {
                    Success = true,
                    Message = AuthMessage.LOGIN_SUCCESS,
                    Data = response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDTO>>> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                
                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<RegisterResponseDTO>
                {
                    Success = true,
                    Message = AuthMessage.REGISTER_SUCCESS,
                    Data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.CONFLICT, new ApiResponse<RegisterResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<RegisterResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request.RefreshToken);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<LoginResponseDTO>
                {
                    Success = true,
                    Message = AuthMessage.REFRESH_SUCCESS,
                    Data = response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] string refreshToken)
        {
            try
            {
                var result = await _authService.LogoutAsync(refreshToken);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<object>
                {
                    Success = true,
                    Message = AuthMessage.LOGOUT_SUCCESS,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Logout failed: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<object>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                var response = await _authService.GoogleLoginAsync(request);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<LoginResponseDTO>
                {
                    Success = true,
                    Message = "Google login thành công",
                    Data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.CONFLICT, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Google login for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("discord-login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> DiscordLogin(
            [FromBody] DiscordLoginRequestDTO request)
        {
            try
            {
                var response = await _authService.DiscordLoginAsync(request);
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<LoginResponseDTO>
                {
                    Success = true,
                    Message = "Discord login thành công",
                    Data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.CONFLICT, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.UNAUTHORIZED, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Discord login for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<LoginResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("send-otp")]
        public async Task<ActionResult<ApiResponse<bool>>> SendOTP([FromBody] SendOTPRequestDTO request)
        {
            try
            {
                var response = await _otpService.SendOTPAsync(request.Email, request.Purpose);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during sending OTP for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyOTP([FromBody] VerifyOTPRequestDTO request)
        {
            try
            {
                var response = await _otpService.VerifyOTPAsync(request.Email, request.OTPCode, request.Purpose);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during verifying OTP for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }

        [HttpPost("complete-registration")]
        public async Task<ActionResult<ApiResponse<bool>>> CompleteRegistration([FromBody] CompleteRegistrationRequestDTO request)
        {
            try
            {
                // Tạo user sau khi verify OTP thành công
                var createUserResult = await _authService.CreateUserAfterOTPVerificationAsync(
                    request.Email, 
                    request.OTPCode,
                    request.RegistrationToken
                );

                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Đăng ký thành công! Tài khoản của bạn đã được tạo.",
                    Data = true
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.CONFLICT, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during completing registration for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }

        [HttpPost("resend-otp")]
        public async Task<ActionResult<ApiResponse<bool>>> ResendOTP([FromBody] SendOTPRequestDTO request)
        {
            try
            {
                var response = await _otpService.ResendOTPAsync(request.Email, request.Purpose);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during resending OTP for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(request.Email);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Mã xác thực đã được gửi đến email của bạn.",
                    Data = true
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request.Email, request.OTPCode, request.NewPassword);
                
                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu thành công!",
                    Data = true
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during reset password for email: {Email}", request.Email);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }

        [HttpGet("get-user-by-supbase-id")]
        public async Task<ActionResult<ApiResponse<User>>> GetUserBySupabaseId([FromQuery] string supabaseId)
        {
            try
            {
                var user = await _authService.GetUserBySupabaseIdAsync(supabaseId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<User>
                {
                    Success = true,
                    Message = "Lấy thông tin người dùng thành công",
                    Data = user
                });
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<User>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with Supabase ID: {SupabaseId}", supabaseId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<User>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }
    }
}
