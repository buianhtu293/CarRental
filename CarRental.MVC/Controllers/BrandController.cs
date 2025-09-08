using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Presentation.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CarRental.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;
        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetBrands()
        {
            var brandList = await _brandService.GetAllBrandsAsync();

            var brands = brandList
                .Select(c => c.BrandName)
                .Distinct()
                .ToList();

            return Json(new { data = brands });
        }

        [HttpGet("{brandName}/models")]
        public async Task<IActionResult> GetModelsByBrand(string brandName)
        {
            var modelList = await _brandService.GetModelsByBrandAsync(brandName);

            var models = modelList
                .Select(c => c.ModelName)
                .Distinct()
                .ToList();

            return Json(new { data = models });
        }
    }
}
