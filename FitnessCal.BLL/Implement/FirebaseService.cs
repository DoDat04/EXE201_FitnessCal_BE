using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FitnessCal.BLL.Define;
using System.IO;
using System.Threading.Tasks;

namespace FitnessCal.BLL.Implement
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly ILogger<FirebaseService> _logger;
        private readonly string _projectId;

        public FirebaseService(IConfiguration configuration, ILogger<FirebaseService> logger)
        {
            _logger = logger;
            _projectId = configuration["Firebase:ProjectId"]
                         ?? throw new InvalidOperationException("Firebase ProjectId not configured.");

            try
            {
                // 🔹 Chỉ tạo FirebaseApp nếu chưa tồn tại
                if (FirebaseApp.DefaultInstance == null)
                {
                    var serviceAccountJson = configuration["Firebase:ServiceAccountKey"];   // JSON dạng chuỗi (Azure)
                    var serviceAccountPath = configuration["Firebase:ServiceAccountKeyPath"]; // File local

                    FirebaseApp app;

                    if (!string.IsNullOrEmpty(serviceAccountJson))
                    {
                        // ✅ Trường hợp chạy trên Azure (dùng biến môi trường)
                        app = FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromJson(serviceAccountJson),
                            ProjectId = _projectId
                        });

                        _logger.LogInformation("Firebase initialized using JSON from environment variable.");
                    }
                    else if (!string.IsNullOrEmpty(serviceAccountPath))
                    {
                        // ✅ Trường hợp chạy local (dùng file JSON)
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), serviceAccountPath);

                        if (!File.Exists(fullPath))
                            throw new FileNotFoundException($"Firebase service account file not found: {fullPath}");

                        app = FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(fullPath),
                            ProjectId = _projectId
                        });

                        _logger.LogInformation("Firebase initialized using local file: {Path}", fullPath);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Neither Firebase:ServiceAccountKey nor Firebase:ServiceAccountKeyPath configured.");
                    }

                    _logger.LogInformation("Firebase Admin SDK initialized successfully for project: {ProjectId}", _projectId);
                }
                else
                {
                    _logger.LogInformation("FirebaseApp already initialized — using existing instance.");
                }

                // 🔹 Luôn gán instance cho FirebaseMessaging
                _firebaseMessaging = FirebaseMessaging.DefaultInstance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK.");
                throw new InvalidOperationException("Failed to initialize Firebase Admin SDK.", ex);
            }
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body)
        {
            if (string.IsNullOrEmpty(fcmToken))
            {
                _logger.LogWarning("FCM token is empty or null.");
                return false;
            }

            try
            {
                var message = new Message
                {
                    Token = fcmToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    }
                };

                var response = await _firebaseMessaging.SendAsync(message);

                _logger.LogInformation("Firebase notification sent successfully: {Response} to token: {Token}", response, fcmToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Firebase notification to token: {Token}", fcmToken);
                return false;
            }
        }
    }
}
