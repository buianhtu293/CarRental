using CarRental.Application.DTOs;
using CarRental.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CarRental.Application.DTOs.Feedback;
using CarRental.MVC.Models.Feedback;
using CarRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CarRental.MVC.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ICarService _carService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService feedbackService, ICarService carService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _carService = carService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult GiveRating(Guid bookingId, Guid carId)
        {
            var dto = new CreateFeedbackDto
            {
                BookingID = bookingId,
                CarID = carId,
                UserID = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> GiveRating(CreateFeedbackDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _feedbackService.AddFeedbackAsync(dto);

                TempData["SuccessMessage"] = "Thank you for your feedback! Your rating has been submitted successfully.";

                return RedirectToAction("Index", "BookingList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your feedback. Please try again.";
                return View(dto);
            }
        }

        #region MyRegion

        /// <summary>
        /// Gets the current logged-in user id (as Guid).
        /// </summary>
        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User id claim missing"));
        // Guid.Parse("7e9ce6f5-321d-4e44-f9b7-08ddda4866db");

        /// <summary>
        /// Whether current user has Administrator role.
        /// </summary>
        private bool IsAdmin => User.IsInRole(UserRoleName.Administrator);

        /// <summary>
        /// Renders the Feedback Report index page (filters + stats + table partial).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = $"{UserRoleName.Administrator},{UserRoleName.CarOwner}")]
        public async Task<IActionResult> Index(
            Guid? carId,
            int? minStars,
            int? maxStars,
            DateTime? fromDate,
            DateTime? toDate,
            string? keyword,
            int page = 1,
            int pageSize = 10)
        {
            // Load car dropdown via CarService.
            var carsTask = IsAdmin
                ? _carService.GetAllCarsAsync()
                : _carService.GetCarsByOwnerAsync(CurrentUserId);

            // Build request DTO
            var request = new FeedbackReportRequestDto
            {
                CarId = carId,
                MinStars = minStars,
                MaxStars = maxStars,
                FromDate = fromDate,
                ToDate = toDate,
                Keyword = keyword,
                Page = Math.Max(1, page),
                PageSize = Math.Clamp(pageSize, 5, 50)
            };

            var reportTask = _feedbackService.GetReportAsync(CurrentUserId, IsAdmin, request);

            await Task.WhenAll(carsTask, reportTask);

            var cars = await carsTask;
            var carList = cars
                .OrderBy(c => c.Brand)
                .ThenBy(c => c.Model)
                .Select(c => (c.Id, Label: $"{c.Brand} {c.Model}{(string.IsNullOrEmpty(c.LicensePlate) ? "" : $" - {c.LicensePlate}")}"))
                .ToList();

            var report = await reportTask;
            var vm = new FeedbackReportIndexViewModel
            {
                CarId = carId,
                MinStars = minStars,
                MaxStars = maxStars,
                FromDate = fromDate,
                ToDate = toDate,
                Keyword = keyword,
                Items = report.Items,
                AverageStars = report.AverageStars,
                Count1Star = report.Count1Star,
                Count2Stars = report.Count2Stars,
                Count3Stars = report.Count3Stars,
                Count4Stars = report.Count4Stars,
                Count5Stars = report.Count5Stars,
                Cars = carList,
                Paging = new ShopNow.Presentation.Models.PagingModel
                {
                    currentpage = page,
                    countpages = (int)Math.Ceiling((double)report.Total / request.PageSize),
                    generateUrl = p => Url.Action(nameof(Table), new
                    {
                        carId,
                        minStars,
                        maxStars,
                        fromDate,
                        toDate,
                        keyword,
                        page = p ?? 1,
                        pageSize = request.PageSize
                    })!
                }
            };

            return View(vm);
        }

        /// <summary>
        /// Returns the table partial containing report rows and paging.
        /// This endpoint is used by AJAX to refresh only the table area.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = $"{UserRoleName.Administrator},{UserRoleName.CarOwner}")]
        public async Task<IActionResult> Table(
            Guid? carId,
            int? minStars,
            int? maxStars,
            DateTime? fromDate,
            DateTime? toDate,
            string? keyword,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var request = new FeedbackReportRequestDto
                {
                    CarId = carId,
                    MinStars = minStars,
                    MaxStars = maxStars,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Keyword = keyword,
                    Page = Math.Max(1, page),
                    PageSize = Math.Clamp(pageSize, 5, 50)
                };

                var report = await _feedbackService.GetReportAsync(CurrentUserId, IsAdmin, request);

                var vm = new FeedbackReportIndexViewModel
                {
                    Items = report.Items,
                    AverageStars = report.AverageStars,
                    Count1Star = report.Count1Star,
                    Count2Stars = report.Count2Stars,
                    Count3Stars = report.Count3Stars,
                    Count4Stars = report.Count4Stars,
                    Count5Stars = report.Count5Stars,
                    Paging = new ShopNow.Presentation.Models.PagingModel
                    {
                        currentpage = page,
                        countpages = (int)Math.Ceiling((double)report.Total / request.PageSize),
                        generateUrl = p => Url.Action(nameof(Table), new
                        {
                            carId,
                            minStars,
                            maxStars,
                            fromDate,
                            toDate,
                            keyword,
                            page = p ?? 1,
                            pageSize = request.PageSize
                        })!
                    }
                };

                return PartialView("~/Views/Feedback/_FeedbackTable.cshtml", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build feedback table partial");
                Response.StatusCode = 500;
                return Content("<div class='alert alert-danger'>Failed to load feedback table. Please try again.</div>", "text/html");
            }
        }

        #endregion
    }
}