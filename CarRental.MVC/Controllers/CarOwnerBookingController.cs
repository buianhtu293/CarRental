using CarRental.Application.DTOs.Booking;
using CarRental.Application.Interfaces;
using CarRental.Domain.Enums;
using CarRental.MVC.Extensions;
using CarRental.MVC.Models.Booking;
using CarRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.MVC.Controllers
{
    [Authorize(Roles = $"{UserRoleName.Administrator},{UserRoleName.CarOwner}")]
    public class CarOwnerBookingController : Controller
    {
        private readonly ICarOwnerBookingService _bookingListService;
        private readonly ILogger<CarOwnerBookingController> _logger;

        public CarOwnerBookingController(
            ICarOwnerBookingService bookingListService,
            ILogger<CarOwnerBookingController> logger)
        {
            _bookingListService = bookingListService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string sortBy = "newest", int? statusFilter = null, string searchTerm = "")
        {
            try
            {
                var userId = GetCurrentUserId();

                //userId = Guid.Parse("7e9ce6f5-321d-4e44-f9b7-08ddda4866db");

                var request = new BookingListRequestDto
                {
                    Page = page,
                    PageSize = 10,
                    SortBy = sortBy,
                    StatusFilter = statusFilter.HasValue ? (BookingItemStatusEnum)statusFilter.Value : null,
                    SearchTerm = searchTerm ?? "",
                    UserId = userId
                };

                var response = await _bookingListService.GetBookingListAsync(request);
                var viewModel = response.ToViewModelCarOwner();

                // Set current filter values
                // This step is realy neeed ?
                viewModel.SortBy = sortBy;
                viewModel.StatusFilter = statusFilter.HasValue ? (BookingItemStatusEnum)statusFilter.Value : null;
                viewModel.SearchTerm = searchTerm ?? "";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking list");
                TempData["ErrorMessage"] = "An error occurred while loading booking list.";
                return View(new BookingListViewModel());
            }
        }

        /// <summary>
        /// Displays detailed information about a specific booking item for the Car Owner.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item.</param>
        /// <returns>Detail view of the booking item.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(Guid bookingItemId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // userId = Guid.Parse("7e9ce6f5-321d-4e44-f9b7-08ddda4866db");

                var response = await _bookingListService.GetBookingListDetailAsync(userId.Value, bookingItemId);
                var viewModel = response.Bookings.First().ToDetailViewModel();

                return View("Details", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Booking item detail");
                TempData["ErrorMessage"] = "An error occurred while loading Booking item detail.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Confirm deposit for a booking item (car owner only).
        /// </summary>
        /// <param name="bookingItemId">BookingItem Id.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeposit([FromForm] Guid bookingItemId)
        {
            var ownerId = GetCurrentUserId();
            //ownerId = Guid.Parse("7e9ce6f5-321d-4e44-f9b7-08ddda4866db");
            if (ownerId is null)
            {
                return Unauthorized(new { success = false, message = "You must be logged in." });
            }

            try
            {
                var ok = await _bookingListService.ConfirmDepositAsync(bookingItemId, ownerId.Value);
                if (ok)
                {
                    return Json(new { success = true, message = "Deposit confirmed successfully." });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Unable to confirm deposit. The item may not exist, you may not be the owner, or status is not PendingDeposit."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming deposit for bookingItemId {BookingItemId}", bookingItemId);
                return StatusCode(500, new { success = false, message = "Internal error while confirming deposit." });
            }
        }

        /// <summary>
        /// Confirm full payment for a booking item (car owner only).
        /// </summary>
        /// <param name="bookingItemId">BookingItem Id.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment([FromForm] Guid bookingItemId)
        {
            var ownerId = GetCurrentUserId();
            //ownerId = Guid.Parse("7e9ce6f5-321d-4e44-f9b7-08ddda4866db");
            if (ownerId is null)
                return Unauthorized(new { success = false, message = "You must be logged in." });

            try
            {
                var ok = await _bookingListService.ConfirmPaymentAsync(bookingItemId, ownerId.Value);
                if (ok)
                {
                    return Json(new { success = true, message = "Payment confirmed successfully." });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Unable to confirm payment. The item may not exist, you may not be the owner, or status is not PendingPayment."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for bookingItemId {BookingItemId}", bookingItemId);
                return StatusCode(500, new { success = false, message = "Internal error while confirming payment." });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }

            return null;
        }
    }
}