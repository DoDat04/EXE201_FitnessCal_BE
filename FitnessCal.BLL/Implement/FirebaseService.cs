using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FitnessCal.BLL.Define;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FitnessCal.BLL.Implement
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly ILogger<FirebaseService> _logger;
        private readonly string _projectId;
        private static readonly object _lock = new();

        public FirebaseService(IConfiguration configuration, ILogger<FirebaseService> logger)
        {
            _logger = logger;
            _projectId = configuration["Firebase:ProjectId"]
                ?? throw new InvalidOperationException("Firebase ProjectId not configured");

            lock (_lock)
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    try
                    {
                        var serviceAccountPath = configuration["Firebase:ServiceAccountKeyPath"];
                        var firebaseJson = configuration["Firebase:ServiceAccountJson"];
                        // Cho phép chọn giữa file hoặc JSON chuỗi

                        GoogleCredential credential;

                        if (!string.IsNullOrEmpty(firebaseJson))
                        {
                            // ✅ Dành cho trường hợp lưu JSON trong App Settings (Azure)
                            firebaseJson = firebaseJson.Replace("\\n", "\n");
                            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(firebaseJson));
                            credential = GoogleCredential.FromStream(stream);
                        }
                        else if (!string.IsNullOrEmpty(serviceAccountPath))
                        {
                            // ✅ Dành cho trường hợp đọc từ file
                            var fullPath = Path.Combine(AppContext.BaseDirectory, serviceAccountPath);
                            if (!File.Exists(fullPath))
                            {
                                throw new FileNotFoundException($"Firebase service account file not found: {fullPath}");
                            }
                            credential = GoogleCredential.FromFile(fullPath);
                        }
                        else
                        {
                            throw new InvalidOperationException("Firebase configuration missing (ServiceAccountKeyPath or ServiceAccountJson)");
                        }

                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = credential,
                            ProjectId = _projectId
                        });

                        _logger.LogInformation("✅ Firebase Admin SDK initialized successfully for project: {ProjectId}", _projectId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to initialize Firebase Admin SDK");
                        throw new InvalidOperationException("Failed to initialize Firebase Admin SDK", ex);
                    }
                }
            }

            _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                {
                    _logger.LogWarning("FCM token is empty");
                    return false;
                }

                var notification = new Message
                {
                    Token = fcmToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    }
                };

                var response = await _firebaseMessaging.SendAsync(notification);
                _logger.LogInformation("📩 Firebase notification sent successfully: {Response} to token: {Token}",
                    response, fcmToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Error sending Firebase notification to token: {Token}", fcmToken);
                return false;
            }
        }
    }
}
