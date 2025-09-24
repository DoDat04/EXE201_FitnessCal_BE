using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.AllergyDTO.Request;
using FitnessCal.BLL.DTO.AllergyDTO.Response;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class AllergyService : IAllergyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AllergyService> _logger;

        public AllergyService(IUnitOfWork unitOfWork, ILogger<AllergyService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CreateAllergyResponseDTO> CreateAllergyAsync(Guid userId, CreateAllergyDTO dto)
        {
            var hasFood = dto.FoodId.HasValue;
            var hasDish = dto.DishId.HasValue;
            if (hasFood == hasDish)
            {
                throw new ArgumentException("Vui lòng chọn đúng 1 mục dị ứng: food hoặc dish");
            }

            var exists = await _unitOfWork.Allergies.ExistsAsync(userId, dto.FoodId, dto.DishId);
            if (exists)
            {
                throw new ArgumentException("Mục dị ứng này đã tồn tại");
            }

            var allergy = new Allergy
            {
                UserId = userId,
                FoodId = dto.FoodId,
                DishId = dto.DishId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Allergies.AddAsync(allergy);
            await _unitOfWork.Save();

            return new CreateAllergyResponseDTO
            {
                AllergyId = allergy.AllergyId,
                Message = "Tạo dị ứng thành công"
            };
        }

        public async Task<IEnumerable<AllergyResponseDTO>> GetUserAllergiesAsync(Guid userId)
        {
            var allergies = await _unitOfWork.Allergies.GetByUserIdAsync(userId);
            
            return allergies.Select(a => new AllergyResponseDTO
            {
                AllergyId = a.AllergyId,
                UserId = a.UserId,
                FoodId = a.FoodId,
                FoodName = a.Food != null ? a.Food.Name : null,
                DishId = a.DishId,
                DishName = a.Dish != null ? a.Dish.Name : null,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            });
        }

        public async Task<UpdateAllergyResponseDTO> UpdateAllergyAsync(int allergyId, UpdateAllergyDTO dto, Guid userId)
        {
            var allergy = await _unitOfWork.Allergies.GetByIdAsync(allergyId);
            if (allergy == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thực phẩm dị ứng");
            }

            // Kiểm tra quyền sở hữu
            if (allergy.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn chỉ có thể cập nhật thực phẩm dị ứng của bạn");
            }

            var hasFood = dto.FoodId.HasValue;
            var hasDish = dto.DishId.HasValue;
            if (hasFood == hasDish)
            {
                throw new ArgumentException("Vui lòng chọn đúng 1 mục dị ứng: food hoặc dish");
            }

            var exists = await _unitOfWork.Allergies.ExistsAsync(allergy.UserId, dto.FoodId, dto.DishId);
            if (exists && (allergy.FoodId != dto.FoodId || allergy.DishId != dto.DishId))
            {
                throw new ArgumentException("Mục dị ứng này đã tồn tại");
            }

            allergy.FoodId = dto.FoodId;
            allergy.DishId = dto.DishId;
            allergy.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Allergies.UpdateAsync(allergy);
            await _unitOfWork.Save();

            return new UpdateAllergyResponseDTO
            {
                Message = "Thực phẩm dị ứng cập nhật thành công"
            };
        }

        public async Task<DeleteAllergyResponseDTO> DeleteAllergyAsync(int allergyId, Guid userId)
        {
            var allergy = await _unitOfWork.Allergies.GetByIdAsync(allergyId);
            if (allergy == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thực phẩm dị ứng");
            }

            // Kiểm tra quyền sở hữu
            if (allergy.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn chỉ có thể xóa thực phẩm dị ứng của bạn");
            }

            await _unitOfWork.Allergies.DeleteAsync(allergy);
            await _unitOfWork.Save();

            return new DeleteAllergyResponseDTO
            {
                Message = "Thực phẩm dị ứng được xóa thành công"
            };
        }

        public async Task<IEnumerable<int>> GetUserAllergyFoodIdsAsync(Guid userId)
        {
            var allergies = await _unitOfWork.Allergies.GetByUserIdAsync(userId);
            return allergies
                .Where(a => a.FoodId.HasValue)
                .Select(a => a.FoodId!.Value);
        }
    }
}
