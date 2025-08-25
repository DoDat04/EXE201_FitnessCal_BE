using FitnessCal.BLL.DTO.MealPlanningDTO;

namespace FitnessCal.BLL.Define
{
    public interface IMealPlanningService
    {
        Task<MealPlanningResponseDTO> GenerateMealPlanAsync(Guid userId);
    }
}
