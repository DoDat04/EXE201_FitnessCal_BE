using FitnessCal.BLL.Constants;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.AuthDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.DTO.UserDTO.Response;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDTO>>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = true,
                    Message = users.Any()
                        ? UserMessage.USER_LIST_RETRIEVED_SUCCESS
                        : UserMessage.USER_LIST_EMPTY,
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPost("ban/{userId}")]
        public async Task<ActionResult<ApiResponse<bool>>> BanUser(Guid userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<bool>
                {
                    Success = true,
                    Message = UserMessage.USER_DELETED_SUCCESS,
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while banning user with ID {UserId}", userId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }

        [HttpPost("unban/{userId}")]
        public async Task<ActionResult<ApiResponse<bool>>> UnBanUser(Guid userId)
        {
            try
            {
                var result = await _userService.UnBanUserAsync(userId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<bool>
                {
                    Success = true,
                    Message = UserMessage.USER_UNBANNED_SUCCESS,
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while banning user with ID {UserId}", userId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<bool>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = false
                });
            }
        }
    }
}
