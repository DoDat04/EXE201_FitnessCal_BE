using FitnessCal.BLL.DTO.FavoriteFoodDTO.Request;
using FitnessCal.BLL.DTO.FavoriteFoodDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IFavoriteFoodService
    {
        Task<CreateFavoriteFoodResponseDTO> CreateFavoriteFoodAsync(Guid userId, CreateFavoriteFoodDTO dto);
        Task<IEnumerable<FavoriteFoodResponseDTO>> GetUserFavoriteFoodsAsync(Guid userId);
        Task<DeleteFavoriteFoodResponseDTO> DeleteFavoriteFoodAsync(int favoriteFoodId, Guid userId);
        Task<IEnumerable<int>> GetUserFavoriteFoodIdsAsync(Guid userId);
    }
}
