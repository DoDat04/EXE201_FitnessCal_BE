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
            var exists = await _unitOfWork.Allergies.ExistsAsync(userId, dto.FoodId);
            if (exists)
            {
                throw new ArgumentException($"Thực phẩm dị ứng này đã tồn tại");
            }

            var allergy = new Allergy
            {
                UserId = userId,
                FoodId = dto.FoodId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Allergies.AddAsync(allergy);
            await _unitOfWork.Save();

            return new CreateAllergyResponseDTO
            {
                AllergyId = allergy.AllergyId,
                Message = "Thực phẩm dị ứng tạo thành công"
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
                FoodName = a.Food.Name,
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

            var exists = await _unitOfWork.Allergies.ExistsAsync(allergy.UserId, dto.FoodId);
            if (exists && allergy.FoodId != dto.FoodId)
            {
                throw new ArgumentException($"Thực phẩm dị ứng này đã tồn tại");
            }

            allergy.FoodId = dto.FoodId;
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
            return allergies.Select(a => a.FoodId);
        }
    }
}
