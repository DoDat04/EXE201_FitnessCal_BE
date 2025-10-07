using FitnessCal.BLL.DTO.UserHealthDTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Define
{
    public interface IUserHealthService
    {
        Task<HealthUserInfoDTO?> GetHealthUserInfoAsync(Guid userId);
    }
}
