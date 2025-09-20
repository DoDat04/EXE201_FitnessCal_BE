using FitnessCal.BLL.DTO.PremiumPackageDTO.Request;
using FitnessCal.DAL.Define;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCal.API.Controllers
{
    [ApiController]
    [Route("api/premium-packages")]
    public class PremiumPackageController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public PremiumPackageController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Chỉ trả về các gói trả phí, không bao gồm gói Free
            var packages = await _uow.PremiumPackages.GetAllAsync();
            var paidPackages = packages.Where(p => p.Price > 0).ToList();
            return Ok(paidPackages);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePremiumPackageRequest updatedPackage)
        {
            var existingPackage = await _uow.PremiumPackages.GetByIdAsync(id);
            if (existingPackage == null)
            {
                return NotFound();
            }

            existingPackage.Price = updatedPackage.Price;

            await _uow.PremiumPackages.UpdateAsync(existingPackage);
            await _uow.Save();

            return NoContent();
        }
    }
}


