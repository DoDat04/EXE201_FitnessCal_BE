using FitnessCal.BLL.DTO.AuthDTO.Request;
using FitnessCal.BLL.DTO.AuthDTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Define
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request);
        Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        Task<LoginResponseDTO> GoogleLoginAsync(GoogleLoginRequestDTO request);
        Task<LoginResponseDTO> DiscordLoginAsync(DiscordLoginRequestDTO request);
        Task<bool> ActivateUserAsync(string email);
        Task<bool> CreateUserAfterOTPVerificationAsync(string email, string otpCode, string registrationToken);
    }
}
