using CarRental.Application.Interfaces;
using CarRental.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.MVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICarService _carService;
        public CustomerController(ICarService carService)
        {
            _carService = carService;
        }
        public IActionResult Index()
        {
            return View(new GuestHomepageViewModel());
        }
        [HttpGet]
        public async Task<IActionResult> ViewDetails(Guid id)
        {
            var idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = idString != null ? Guid.Parse(idString) : (Guid?)null;

            var vm = await _carService.GetDetailsAsync(id, currentUserId);
            if (vm == null) return NotFound();

            return View(vm); 
        }
    }
}
