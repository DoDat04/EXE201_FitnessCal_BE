using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace FitnessCal.BLL.Implement
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(HttpClient httpClient, IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> SendEmailAsync(string to, string subject, string htmlContent)
        {
            try
            {
                var requestData = new
                {
                    from = _emailSettings.FromEmail,
                    to = new[] { to },
                    subject = subject,
                    html = htmlContent
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_emailSettings.ApiKey}");

                var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Email sent successfully to {to}");
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Email sent successfully",
                        Data = true
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send email to {to}. Status: {response.StatusCode}, Error: {errorContent}");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Failed to send email: {response.StatusCode}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error sending email: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> GuestSendEmailAsync(string from, string subject, string htmlContent)
        {
            try
            {
                var requestData = new
                {
                    from = _emailSettings.FromEmail,   
                    to = _emailSettings.ToEmail,
                    subject = subject,
                    html = htmlContent,
                    reply_to = from                  
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_emailSettings.ApiKey}");

                var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Guest email sent successfully from {from}");
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Email sent successfully",
                        Data = true
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send guest email from {from}. Status: {response.StatusCode}, Error: {errorContent}");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Failed to send email: {response.StatusCode}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending guest email from {from}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error sending email: {ex.Message}",
                    Data = false
                };
            }
        }

    }
}
