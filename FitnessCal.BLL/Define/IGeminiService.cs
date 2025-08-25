namespace FitnessCal.BLL.Define
{
    public interface IGeminiService
    {
        Task<string> GenerateMealPlanAsync(string prompt);
    }
}
