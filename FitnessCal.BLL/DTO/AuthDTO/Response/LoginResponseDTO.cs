using System.Text.Json.Serialization;

namespace FitnessCal.BLL.DTO.AuthDTO.Response
{
    public class LoginResponseDTO
    {
        [JsonPropertyName("access_token")]  
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")] 
        public string RefreshToken { get; set; } = string.Empty;
    }
}
