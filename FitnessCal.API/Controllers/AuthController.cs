using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.AuthDTO.Request;
using FitnessCal.BLL.DTO.AuthDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;

namespace FitnessCal.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
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
    }
}
