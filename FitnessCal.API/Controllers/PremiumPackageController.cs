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
            var packages = await _uow.PremiumPackages.GetAllAsync();
            return Ok(packages);
        }
    }
}


