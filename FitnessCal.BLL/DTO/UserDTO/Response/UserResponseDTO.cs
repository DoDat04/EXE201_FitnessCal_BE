using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.UserDTO.Response
{
    public class UserResponseDTO
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public short IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
