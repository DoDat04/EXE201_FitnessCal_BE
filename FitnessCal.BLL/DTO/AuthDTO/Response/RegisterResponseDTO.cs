using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.AuthDTO.Response
{
    public class RegisterResponseDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RegistrationToken { get; set; } = string.Empty;
    }
}
