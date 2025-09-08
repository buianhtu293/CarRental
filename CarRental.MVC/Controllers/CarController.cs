using CarRental.Application.DTOs;
using CarRental.Application.Interfaces;
using CarRental.Domain.Models;
using CarRental.MVC.Models;
using CarRental.MVC.Models.Car;
using CarRental.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IBrandService _brandService;
        private readonly HandleSessionService _sessionService;
        private readonly IPhotoService _photoService;

        public CarsController(ICarService carService, IBrandService brandService,
            HandleSessionService sessionService, IPhotoService photoService)
        {
            _carService = carService;
            _brandService = brandService;
            _sessionService = sessionService;
            _photoService = photoService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CarDto>>> GetAllCars()
        {
            var cars = await _carService.GetAllCarsAsync();
            return Ok(cars);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CarDto>> GetCar(int id)
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return Ok(car);
        }

        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CarDto>>> GetAvailableCars([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            IEnumerable<CarDto> cars;

            if (startDate.HasValue && endDate.HasValue)
            {
                cars = await _carService.GetAvailableCarsAsync(startDate.Value, endDate.Value);
            }
            else
            {
                cars = await _carService.GetAvailableCarsAsync();
            }

            return Ok(cars);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CarDto>> UpdateCar(int id, [FromBody] CreateCarDto updateCarDto)
        {
            var car = await _carService.UpdateCarAsync(id, updateCarDto);
            return Ok(car);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<CarDto>> UpdateCarStatus(int id, [FromBody] string status)
        {
            var car = await _carService.UpdateCarStatusAsync(id, status);
            return Ok(car);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            await _carService.DeleteCarAsync(id);
            return NoContent();
        }
    }
}

public class CarController : Controller
{
    private readonly ICarService _carService;
    private readonly IBrandService _brandService;
    private readonly HandleSessionService _sessionService;
    private readonly IPhotoService _photoService;

    public CarController(ICarService carService, IBrandService brandService,
        HandleSessionService sessionService, IPhotoService photoService)
    {
        _carService = carService;
        _brandService = brandService;
        _sessionService = sessionService;
        _photoService = photoService;
    }

    public async Task<IActionResult> Index()
    {
        var cars = await _carService.GetAllCarsAsync();
        return View(cars);
    }

    public async Task<IActionResult> Details(int id)
    {
        var car = await _carService.GetCarByIdAsync(id);
        if (car == null)
        {
            return NotFound();
        }
        return View(car);
    }

    public async Task<IActionResult> Search(
        [FromQuery] SearchFormModel criteria,
        int page = 1,
        string view = "grid",
        string sort = "newest")
    {
        if (!ModelState.IsValid || criteria.CombinedReturnDateTime <= criteria.CombinedPickupDateTime)
        {
            if (criteria.CombinedReturnDateTime <= criteria.CombinedPickupDateTime)
            {
                ModelState.AddModelError("", "Thời gian trả xe phải sau thời gian nhận xe.");
            }

            var emptyResult = new PagedResult<CarSearchDto>(new List<CarSearchDto>(), 0, 1, 10);
            var errorViewModel = new SearchViewModel
            {
                SearchCriteria = criteria,
                SearchResult = emptyResult,
                CurrentView = view,
                CurrentSort = sort
            };
            return View("SearchResults", errorViewModel);
        }

        const int pageSize = 10;

        var pagedResult = await _carService.SearchCarsAsync(criteria, page, pageSize);

        var viewModel = new SearchViewModel
        {
            SearchCriteria = criteria,
            SearchResult = pagedResult,
            CurrentView = view,
            CurrentSort = sort
        };

        return View("SearchResults", viewModel);
    }

    #region Create Car

    [HttpGet]
    public IActionResult Create()
    {
        var model = new CarCreateViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CarCreateViewModel model)
    {
        Console.WriteLine(model.Address);

        if (model == null)
            return BadRequest("Invalid data");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        CreateCarDto carDto = MapToDto(model);

        // Save car
        await _carService.CreateCarAsync(carDto);
        HttpContext.Session.Remove("CreateCarData");

        return Ok(new { success = true, redirectUrl = Url.Action("MyCars", "Car") });
    }

    [HttpGet]
    public IActionResult Cancel()
    {
        // Delete session (temp data)
        HttpContext.Session.Remove("CarCreateData");
        return RedirectToAction("Owner", "Home"); // redirect to my cars page
    }

    #endregion

    #region Get My Cars

    [HttpGet]
    public async Task<IActionResult> MyCars(bool newToOld = true, int page = 1, int pageSize = 10)
    {
        var pagedResult = await _carService.GetMyCarCarsAsync(newToOld, page, pageSize, GetCurrentUserId().Value);

        ViewBag.NewToOld = newToOld;

        return View("MyCars", pagedResult);
    }

    #endregion

    #region Edit Car

    [HttpGet]
    public async Task<IActionResult> Edit(Guid carId)
    {
        EditCarDto car = await _carService.GetCarDetailsAsync(carId);

        if (car == null)
        {
            return NotFound();
        }

        return View("Edit", car);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        await _carService.UpdateCarStatus(id, status);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> EditDetails(EditCarDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data" });
        }

        // duyệt qua CarDocuments và upload file nếu có
        foreach (var doc in model.CarDocuments ?? new List<CarDocumentDto>())
        {
            if (doc.File != null && doc.File.Length > 0)
            {
                // Upload file lên cloud
                var cloudUrl = await _photoService.AddPhotoAsync(doc.File);

                // thay FilePath bằng url trên cloud
                doc.FilePath = cloudUrl.SecureUrl.AbsoluteUri;
            }
        }

        await _carService.UpdateCarDetails(model);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> EditPricing(EditCarDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data" });
        }

        await _carService.UpdateCarPricing(model);

        return Json(new { success = true });
    }

    #endregion

    #region Helper methods

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out Guid userId))
        {
            return userId;
        }
        return new Guid("39835EF4-E24E-4D92-F1CA-08DDDEF5268C");
    }

    public CreateCarDto MapToDto(CarCreateViewModel model)
    {
        if (model == null) return null!;

        return new CreateCarDto
        {
            Id = Guid.NewGuid(), // tạo Id mới cho Car
            OwnerId = (Guid)GetCurrentUserId(),
            Brand = model.Brand ?? string.Empty,
            Model = model.Model ?? string.Empty,
            ProductionYear = model.ProductionYear ?? 0,
            Color = model.Color ?? string.Empty,
            LicensePlate = model.LicensePlate ?? string.Empty,
            Transmission = model.Transmission ?? string.Empty,
            FuelType = model.FuelType ?? string.Empty,
            Seats = model.Seats ?? 0,

            Mileage = model.Mileage ?? 0,
            Province = model.Province ?? string.Empty,
            District = model.District ?? string.Empty,
            Ward = model.Ward ?? string.Empty,
            Address = model.Address ?? string.Empty,

            BasePricePerDay = model.BasePricePerDay ?? 0,
            RequiredDeposit = model.RequiredDeposit ?? 0,
            Description = model.Description,

            Status = model.Status ?? "Available",

            CarDocuments = model.CarDocuments?.ToList() ?? new List<CarDocumentDto>(),
            CarSpecifications = model.CarSpecifications?.ToList() ?? new List<CarSpecificationDto>()
        };
    }

    [HttpGet]
    public async Task<IActionResult> CheckLicensePlate(Guid? id, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return Json(new { valid = false, message = "LicensePlate is required" });

        var exists = await _carService.CheckAsync(id, licensePlate);

        if (exists)
            return Json(new { valid = false, message = "LicensePlate already exists" });

        return Json(new { valid = true });
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> files)
    {
        if (files == null || !files.Any())
            return BadRequest("Không có file nào được gửi");

        var urls = new List<string>();

        foreach (var file in files)
        {
            var cloudUrl = await _photoService.AddPhotoAsync(file);
            urls.Add(cloudUrl.SecureUrl.AbsoluteUri);
        }

        return Ok(urls);
    }

    #endregion
}