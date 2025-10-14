using FitnessCal.BLL.Constants;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Helpers;
using FitnessCal.BLL.Transformer;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FitnessCal.BLL.Implement.FoodService;

namespace FitnessCal.BLL.Implement
{
    public class UserCapturedFoodService : IUserCapturedFoodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserCapturedFoodService> _logger;
        private readonly CurrentUserIdHelper _currentUserIdHelper;
        private readonly SaveTrainingData _saveTrainingData;
        public UserCapturedFoodService(IUnitOfWork unitOfWork, ILogger<UserCapturedFoodService> logger, 
            CurrentUserIdHelper currentUserIdHelper, SaveTrainingData saveTrainingData)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserIdHelper = currentUserIdHelper;
            _saveTrainingData = saveTrainingData;
        }
        public async Task<ApiResponse<object>> ConfirmCapturedFood(ParsedFoodInfo foodInfo, string imageUrl)
        {
            try
            {
                var userId = _currentUserIdHelper.GetCurrentUserId();

                await _saveTrainingData.SaveTrainingDataAsync(foodInfo);

                var userCapturedFood = new UserCapturedFood
                {
                    UserId = userId,
                    Name = foodInfo.Name,
                    Calories = foodInfo.Calories,
                    Carbs = foodInfo.Carbs,
                    Fat = foodInfo.Fat,
                    Protein = foodInfo.Protein
                };

                await _unitOfWork.UserCapturedFoods.AddAsync(userCapturedFood);
                await _unitOfWork.Save();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Đã xác nhận và lưu món ăn vào nhật ký",
                    Data = new
                    {
                        FoodId = userCapturedFood.Id,
                        foodInfo.Name,
                        foodInfo.Calories,
                        foodInfo.Carbs,
                        foodInfo.Fat,
                        foodInfo.Protein,
                        ImageUrl = imageUrl
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmCapturedFood failed");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                };
            }
        }

    }
}
