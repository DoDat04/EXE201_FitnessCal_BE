using Microsoft.AspNetCore.Mvc;
using FitnessCal.BLL.Define;
using FitnessCal.BLL.DTO.AllergyDTO.Request;
using FitnessCal.BLL.DTO.AllergyDTO.Response;
using FitnessCal.BLL.DTO.CommonDTO;
using FitnessCal.BLL.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCal.API.Controllers
{
    [Route("api/allergies")]
    [ApiController]
    [Authorize]
    public class AllergyController : ControllerBase
    {
        private readonly IAllergyService _allergyService;
        private readonly ILogger<AllergyController> _logger;

        public AllergyController(IAllergyService allergyService, ILogger<AllergyController> logger)
        {
            _allergyService = allergyService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateAllergyResponseDTO>>> CreateAllergy([FromBody] CreateAllergyDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _allergyService.CreateAllergyAsync(userId, dto);

                return StatusCode(ResponseCodes.StatusCodes.CREATED, new ApiResponse<CreateAllergyResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in CreateAllergy: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<CreateAllergyResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating allergy");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<CreateAllergyResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AllergyResponseDTO>>>> GetUserAllergies()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _allergyService.GetUserAllergiesAsync(userId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<IEnumerable<AllergyResponseDTO>>
                {
                    Success = true,
                    Message = "Allergies retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user allergies");
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<IEnumerable<AllergyResponseDTO>>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpPut("{allergyId}")]
        public async Task<ActionResult<ApiResponse<UpdateAllergyResponseDTO>>> UpdateAllergy(int allergyId, [FromBody] UpdateAllergyDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _allergyService.UpdateAllergyAsync(allergyId, dto, userId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<UpdateAllergyResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Allergy not found in UpdateAllergy: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<UpdateAllergyResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in UpdateAllergy: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.BAD_REQUEST, new ApiResponse<UpdateAllergyResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in UpdateAllergy: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.FORBIDDEN, new ApiResponse<UpdateAllergyResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating allergy {AllergyId}", allergyId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<UpdateAllergyResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        [HttpDelete("{allergyId}")]
        public async Task<ActionResult<ApiResponse<DeleteAllergyResponseDTO>>> DeleteAllergy(int allergyId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _allergyService.DeleteAllergyAsync(allergyId, userId);

                return StatusCode(ResponseCodes.StatusCodes.OK, new ApiResponse<DeleteAllergyResponseDTO>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Allergy not found in DeleteAllergy: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.NOT_FOUND, new ApiResponse<DeleteAllergyResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in DeleteAllergy: {Message}", ex.Message);
                return StatusCode(ResponseCodes.StatusCodes.FORBIDDEN, new ApiResponse<DeleteAllergyResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting allergy {AllergyId}", allergyId);
                return StatusCode(ResponseCodes.StatusCodes.INTERNAL_SERVER_ERROR, new ApiResponse<DeleteAllergyResponseDTO>
                {
                    Success = false,
                    Message = ResponseCodes.Messages.INTERNAL_ERROR,
                    Data = null
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }
    }
}
