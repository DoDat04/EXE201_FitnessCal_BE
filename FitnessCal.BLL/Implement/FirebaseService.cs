using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FitnessCal.BLL.Define;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

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
            _projectId = configuration["Firebase:ProjectId"] ?? throw new InvalidOperationException("Firebase ProjectId not configured");

            if (FirebaseApp.DefaultInstance == null)
            {
                try
                {
                    FirebaseApp app;
                    var serviceAccountJson = configuration["Firebase:ServiceAccountKey"]; // JSON dạng chuỗi (Azure)
                    var serviceAccountPath = configuration["Firebase:ServiceAccountKeyPath"]; // file local

                    if (!string.IsNullOrEmpty(serviceAccountJson))
                    {
                        // ✅ Trường hợp dùng biến môi trường (Azure)
                        app = FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromJson(serviceAccountJson),
                            ProjectId = _projectId
                        });
                        _logger.LogInformation("Firebase initialized using JSON from environment variable.");
                    }
                    else if (!string.IsNullOrEmpty(serviceAccountPath))
                    {
                        // ✅ Trường hợp chạy local (file thật)
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
                        throw new InvalidOperationException("Neither Firebase:ServiceAccountKey nor Firebase:ServiceAccountKeyPath configured");
                    }

                    _logger.LogInformation("Firebase Admin SDK initialized successfully for project: {ProjectId}", _projectId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
                    throw new InvalidOperationException("Failed to initialize Firebase Admin SDK", ex);
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

                _logger.LogInformation("Firebase notification sent successfully: {Response} to token: {Token}",
                    response, fcmToken);

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
