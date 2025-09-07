using FitnessCal.BLL.DTO.SubscriptionDTO.Response;
using FitnessCal.BLL.Define;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCal.BLL.Implement
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserSubscriptionResponseDTO>> GetAllUserSubscriptionsAsync()
        {
            var subscriptions = await _unitOfWork.UserSubscriptions.GetAllAsync(s => s.PaymentStatus == "paid");
            
            // Lấy tất cả UserIds và PackageIds cần thiết
            var userIds = subscriptions.Select(s => s.UserId).Distinct().ToList();
            var packageIds = subscriptions.Select(s => s.PackageId).Distinct().ToList();
            
            // Load tất cả Users và Packages trong một lần
            var users = await _unitOfWork.Users.GetAllAsync(u => userIds.Contains(u.UserId));
            var packages = await _unitOfWork.PremiumPackages.GetAllAsync(p => packageIds.Contains(p.PackageId));
            
            // Tạo dictionaries để lookup nhanh
            var userDict = users.ToDictionary(u => u.UserId, u => u);
            var packageDict = packages.ToDictionary(p => p.PackageId, p => p);
            
            return subscriptions.Select(s => MapToResponseDTO(s, userDict, packageDict)).ToList();
        }
        
        public async Task<UserSubscriptionResponseDTO> GetUserSubscriptionByIdAsync(Guid userId)
        {
            var subscription = await _unitOfWork.UserSubscriptions
                .GetAllAsync(s => s.UserId == userId && s.PaymentStatus == "paid");

            var userSubscriptions = subscription as UserSubscription[] ?? subscription.ToArray();
            if (subscription == null || !userSubscriptions.Any())
                throw new KeyNotFoundException("Không tìm thấy subscription cho người dùng đã cho.");

            var latestSubscription = userSubscriptions.OrderByDescending(s => s.EndDate).FirstOrDefault();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (latestSubscription != null)
            {
                var package = await _unitOfWork.PremiumPackages.GetByIdAsync(latestSubscription.PackageId);

                if (user != null)
                {
                    var userDict = new Dictionary<Guid, User> { { userId, user } };
                    if (package != null)
                    {
                        var packageDict = new Dictionary<int, PremiumPackage> { { latestSubscription.PackageId, package } };

                        return MapToResponseDTO(latestSubscription, userDict, packageDict);
                    }
                }
            }
            throw new KeyNotFoundException("Không tìm thấy subscription cho người dùng đã cho.");
        }

        private UserSubscriptionResponseDTO MapToResponseDTO(UserSubscription subscription, Dictionary<Guid, User> userDict, Dictionary<int, PremiumPackage> packageDict)
        {
            var isActive = subscription.EndDate > DateTime.Now && subscription.PaymentStatus == "paid";
            var daysRemaining = isActive ? (int)(subscription.EndDate - DateTime.Now).TotalDays : 0;

            var user = userDict.TryGetValue(subscription.UserId, out var foundUser) ? foundUser : null;
            var package = packageDict.TryGetValue(subscription.PackageId, out var foundPackage) ? foundPackage : null;

            return new UserSubscriptionResponseDTO
            {
                SubscriptionId = subscription.SubscriptionId,
                UserId = subscription.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                UserEmail = user?.Email ?? "Unknown",
                Package = package != null ? new PackageInfoDTO
                {
                    PackageId = package.PackageId,
                    Name = package.Name,
                    DurationMonths = package.DurationMonths,
                    Price = package.Price,
                    PackageType = package.Price == 0 ? "Free" : "Premium"
                } : new PackageInfoDTO
                {
                    PackageId = 0,
                    Name = "Unknown",
                    DurationMonths = 0,
                    Price = 0,
                    PackageType = "Unknown"
                },
                PriceAtPurchase = subscription.PriceAtPurchase,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                PaymentStatus = subscription.PaymentStatus,
                IsActive = isActive,
                DaysRemaining = daysRemaining,
                IsUserBanned = user != null ? user.IsActive == 0 : false
            };
        }
    }
}
