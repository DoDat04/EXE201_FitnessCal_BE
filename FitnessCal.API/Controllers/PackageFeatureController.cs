using FitnessCal.BLL.DTO.PackageFeatureDTO.Request;
using FitnessCal.DAL.Define;
using FitnessCal.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/package-features")]
    public class PackageFeatureController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public PackageFeatureController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var features = await _uow.PackageFeatures.GetAllAsync();
            var ordered = features
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new { f.Id, f.FeatureName, f.IsActive, f.DisplayOrder })
                .ToList();
            return Ok(ordered);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var feature = await _uow.PackageFeatures.GetByIdAsync(id);
            if (feature == null) return NotFound();
            return Ok(new { feature.Id, feature.FeatureName, feature.IsActive, feature.DisplayOrder });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePackageFeatureRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FeatureName))
                return BadRequest("FeatureName is required");

            // Tự động gán DisplayOrder = max hiện tại + 1, bỏ qua giá trị client gửi
            var existing = await _uow.PackageFeatures.GetAllAsync();
            var maxOrder = existing.Any() ? existing.Max(f => f.DisplayOrder) : 0;

            var entity = new PackageFeature
            {
                FeatureName = request.FeatureName.Trim(),
                IsActive = request.IsActive,
                DisplayOrder = maxOrder + 1
            };
            await _uow.PackageFeatures.AddAsync(entity);
            await _uow.Save();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new { entity.Id, entity.FeatureName, entity.IsActive, entity.DisplayOrder });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePackageFeatureRequest request)
        {
            var entity = await _uow.PackageFeatures.GetByIdAsync(id);
            if (entity == null) return NotFound();

            // Chỉ cho phép cập nhật tên tính năng; không thay đổi IsActive/DisplayOrder tại đây
            if (!string.IsNullOrWhiteSpace(request.FeatureName))
            {
                entity.FeatureName = request.FeatureName.Trim();
            }

            await _uow.PackageFeatures.UpdateAsync(entity);
            await _uow.Save();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.PackageFeatures.GetByIdAsync(id);
            if (entity == null) return NotFound();
            entity.IsActive = !entity.IsActive;
            await _uow.PackageFeatures.UpdateAsync(entity);
            await _uow.Save();
            return Ok(new { entity.Id, entity.IsActive });
        }
    }
}


