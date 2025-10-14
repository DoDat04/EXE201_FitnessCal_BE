using FitnessCal.BLL.DTO.CommonDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FitnessCal.BLL.Implement.FoodService;

namespace FitnessCal.BLL.Define
{
    public interface IUserCapturedFoodService
    {
        Task<ApiResponse<object>> ConfirmCapturedFood(ParsedFoodInfo foodInfo, string imageUrl);
    }
}
