using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCal.BLL.DTO.UserMealItemDTO.Request;
using FitnessCal.BLL.DTO.UserMealItemDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IUserMealItemService
    {
        Task<AddMealItemResponseDTO> AddMealItemAsync(AddMealItemDTO dto);
        Task<DeleteMealItemResponseDTO> DeleteMealItemAsync(int itemId);
        Task<UpdateMealItemResponseDTO> UpdateMealItemAsync(int itemId, UpdateMealItemDTO dto);
    }
}
