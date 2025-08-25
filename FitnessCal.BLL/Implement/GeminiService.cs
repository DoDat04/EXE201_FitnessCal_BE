using FitnessCal.BLL.Define;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FitnessCal.BLL.Implement
{
    public class GeminiService : IGeminiService
    {
        private readonly GoogleAI _api;
        private readonly GenerativeModel _model;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
        {
            var apiKey = configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured");
            }

            _api = new GoogleAI(apiKey: apiKey);
            _model = _api.GenerativeModel("gemini-1.5-pro");
            _logger = logger;
        }

        public async Task<string> GenerateMealPlanAsync(string prompt)
        {
            try
            {
                var result = await _model.GenerateContent(prompt);
                var response = result?.Text ?? string.Empty;

                _logger.LogInformation("Gemini API response generated successfully");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling Gemini API");
                throw new InvalidOperationException("Failed to generate meal plan from Gemini API", ex);
            }
        }
    }
}
