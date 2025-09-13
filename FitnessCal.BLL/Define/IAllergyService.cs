using FitnessCal.BLL.DTO.AllergyDTO.Request;
using FitnessCal.BLL.DTO.AllergyDTO.Response;

namespace FitnessCal.BLL.Define
{
    public interface IAllergyService
    {
        Task<CreateAllergyResponseDTO> CreateAllergyAsync(Guid userId, CreateAllergyDTO dto);
        Task<IEnumerable<AllergyResponseDTO>> GetUserAllergiesAsync(Guid userId);
        Task<UpdateAllergyResponseDTO> UpdateAllergyAsync(int allergyId, UpdateAllergyDTO dto, Guid userId);
        Task<DeleteAllergyResponseDTO> DeleteAllergyAsync(int allergyId, Guid userId);
        Task<IEnumerable<int>> GetUserAllergyFoodIdsAsync(Guid userId);
    }
}
