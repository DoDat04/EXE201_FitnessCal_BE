using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.AuthDTO.Request
{
    public class GoogleTokenRequestDTO
    {
        public string IdToken { get; set; }
        public string GoogleAccessToken { get; set; } = string.Empty;
        public string GoogleRefreshToken { get; set; } = string.Empty;
    }
}
