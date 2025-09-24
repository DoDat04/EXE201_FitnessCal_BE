using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.FavoriteFoodDTO.Request;
using FitnessCal.BLL.DTO.FavoriteFoodDTO.Response;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;


namespace FitnessCal.BLL.Implement
{
    public class FavoriteFoodService : IFavoriteFoodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FavoriteFoodService> _logger;

        public FavoriteFoodService(IUnitOfWork unitOfWork, ILogger<FavoriteFoodService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CreateFavoriteFoodResponseDTO> CreateFavoriteFoodAsync(Guid userId, CreateFavoriteFoodDTO dto)
        {
            var hasFood = dto.FoodId.HasValue;
            var hasDish = dto.DishId.HasValue;
            if (hasFood == hasDish)
            {
                throw new ArgumentException("Vui lòng chọn đúng 1 mục: food hoặc dish");
            }

            var exists = await _unitOfWork.FavoriteFoods.ExistsAsync(userId, dto.FoodId, dto.DishId);
            if (exists)
            {
                throw new ArgumentException("Mục yêu thích này đã tồn tại");
            }

            var favoriteFood = new FavoriteFood
            {
                UserId = userId,
                FoodId = dto.FoodId,
                DishId = dto.DishId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FavoriteFoods.AddAsync(favoriteFood);
            await _unitOfWork.Save();

            return new CreateFavoriteFoodResponseDTO
            {
                FavoriteId = favoriteFood.FavoriteId,
                Message = "Thêm vào yêu thích thành công"
            };
        }

        public async Task<IEnumerable<FavoriteFoodResponseDTO>> GetUserFavoriteFoodsAsync(Guid userId)
        {
            var favoriteFoods = await _unitOfWork.FavoriteFoods.GetByUserIdAsync(userId);

            return favoriteFoods.Select(f => new FavoriteFoodResponseDTO
            {
                FavoriteId = f.FavoriteId,
                UserId = f.UserId,
                FoodId = f.FoodId,
                FoodName = f.Food != null ? f.Food.Name : null,
                DishId = f.DishId,
                DishName = f.PredefinedDish != null ? f.PredefinedDish.Name : null,
                CreatedAt = f.CreatedAt
            });
        }

        public async Task<DeleteFavoriteFoodResponseDTO> DeleteFavoriteFoodAsync(int favoriteFoodId, Guid userId)
        {
            var favoriteFood = await _unitOfWork.FavoriteFoods.GetByIdAsync(favoriteFoodId);
            if (favoriteFood == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thực phẩm yêu thích");
            }

            if (favoriteFood.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn chỉ có thể xóa thực phẩm yêu thích của bạn");
            }

            await _unitOfWork.FavoriteFoods.DeleteAsync(favoriteFood);
            await _unitOfWork.Save();

            return new DeleteFavoriteFoodResponseDTO
            {
                Message = "Thực phẩm yêu thích xóa thành công"
            };
        }

        public async Task<IEnumerable<int>> GetUserFavoriteFoodIdsAsync(Guid userId)
        {
            var favoriteFoodIds = await _unitOfWork.FavoriteFoods.GetByUserIdAsync(userId);
            return favoriteFoodIds.Where(f => f.FoodId.HasValue).Select(f => f.FoodId!.Value);
        }
    }
}
