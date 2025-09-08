using CarRental.Application.DTOs.Booking;
using CarRental.Application.Interfaces;
using CarRental.Domain.Enums;
using CarRental.MVC.Extensions;
using CarRental.MVC.Models.Booking;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.MVC.Controllers
{
    /// <summary>
    /// Handles booking list management including viewing, editing, status updates,
    /// and car return processing for customer booking management interface.
    /// </summary>
    public class BookingListController : Controller
    {
        private readonly IBookingListService _bookingListService;
        private readonly ILogger<BookingListController> _logger;
        private readonly IPhotoService _photoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingListController"/> class.
        /// </summary>
        /// <param name="bookingListService">The booking list service for business operations.</param>
        /// <param name="logger">The logger for tracking controller operations.</param>
        /// <param name="photoService">The photo service for license image management.</param>
        public BookingListController(IBookingListService bookingListService, ILogger<BookingListController> logger, IPhotoService photoService)
        {
            _bookingListService = bookingListService;
            _logger = logger;
            _photoService = photoService;
        }


        #region Booking List Actions

        /// <summary>
        /// Displays a paginated list of user bookings with filtering and sorting options.
        /// Supports search functionality, status filtering, and multiple sorting criteria.
        /// </summary>
        /// <param name="page">The page number for pagination (default: 1).</param>
        /// <param name="sortBy">The sorting criteria for booking list (default: "newest").</param>
        /// <param name="statusFilter">Optional status filter for booking items.</param>
        /// <param name="searchTerm">Optional search term for filtering bookings.</param>
        /// <returns>The booking list view with filtered and paginated results.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string sortBy = "newest", int? statusFilter = null, string searchTerm = "")
        {
            try
            {
                var userId = GetCurrentUserId();

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
                var viewModel = response.ToViewModel();

                // Set current filter values
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
        /// Displays detailed information for a specific booking including all related data.
        /// Shows comprehensive booking details, car information, and current status.
        /// </summary>
        /// <param name="id">The unique identifier of the booking item to display.</param>
        /// <returns>The booking details view or redirect to index if not found.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var booking = await _bookingListService.GetBookingDetailAsync(id, userId.Value);
                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Cần convert BookingDetailDto sang BookingItemViewModel thay vì BookingEditViewModel
                var viewModel = booking.ToDetailsViewModel();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking details for ID {BookingId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading booking details.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays the booking edit interface for modifying booking information.
        /// Allows editing of renter and driver details for confirmed bookings only.
        /// </summary>
        /// <param name="id">The unique identifier of the booking item to edit.</param>
        /// <returns>The booking edit view or redirect based on editability status.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var booking = await _bookingListService.GetBookingDetailAsync(id, userId.Value);
                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (!booking.IsEditable)
                {
                    TempData["ErrorMessage"] = "This booking cannot be edited in its current status.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var viewModel = booking.ToEditViewModel();
                viewModel.Progress.CurrentStep = 1; // Start at information step
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking edit for ID {BookingId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading booking for edit.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes booking information updates including license image uploads.
        /// Handles renter and driver information modifications with validation and persistence.
        /// </summary>
        /// <param name="id">The unique identifier of the booking item to update.</param>
        /// <param name="information">The updated booking information from the form.</param>
        /// <returns>JSON result indicating success or failure of the update operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInformation(Guid id, BookingInformationEditViewModel information)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Handle license image uploads BEFORE updating - sử dụng LicenseImageFile và LicenseImage
                try
                {
                    // Upload renter license image if provided
                    if (information.Renter.LicenseImageFile != null && information.Renter.LicenseImageFile.Length > 0)
                    {
                        var renterResult = await _photoService.AddPhotoAsync(information.Renter.LicenseImageFile);
                        if (renterResult.Error == null)
                        {
                            information.Renter.LicenseImage = renterResult.SecureUrl.AbsoluteUri;
                        }
                    }
                    // If no new image and LicenseImage is empty, keep existing or set to empty
                    else if (string.IsNullOrEmpty(information.Renter.LicenseImage))
                    {
                        information.Renter.LicenseImage = ""; // Explicitly set to empty if removed
                    }

                    // Upload driver license images if provided
                    if (information.Drivers != null)
                    {
                        for (int i = 0; i < information.Drivers.Count; i++)
                        {
                            var driver = information.Drivers[i];
                            if (!driver.IsSameAsRenter && driver.LicenseImageFile != null && driver.LicenseImageFile.Length > 0)
                            {
                                var driverResult = await _photoService.AddPhotoAsync(driver.LicenseImageFile);
                                if (driverResult.Error == null)
                                {
                                    driver.LicenseImage = driverResult.SecureUrl.AbsoluteUri;
                                }
                            }
                            // If no new image and LicenseImage is empty, keep existing or set to empty
                            else if (string.IsNullOrEmpty(driver.LicenseImage))
                            {
                                driver.LicenseImage = ""; // Explicitly set to empty if removed
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading license images during booking update");
                    // Continue without failing the update process
                }

                var updateDto = new BookingDetailUpdateDto
                {
                    PickupDate = information.PickupDate,
                    ReturnDate = information.ReturnDate,
                    RenterInfo = new RenterInformationDto
                    {
                        FullName = information.Renter.FullName,
                        Email = information.Renter.Email,
                        PhoneNumber = information.Renter.PhoneNumber,
                        DateOfBirth = information.Renter.DateOfBirth,
                        LicenseNumber = information.Renter.LicenseNumber,
                        LicenseImageUrl = information.Renter.LicenseImage, // Mapping LicenseImage thành LicenseImageUrl cho DTO
                        Address = information.Renter.Address,
                        City = information.Renter.City,
                        District = information.Renter.District,
                        Ward = information.Renter.Ward
                    },
                    Drivers = information.Drivers?.Select(driver => new DriverInformationDto
                    {
                        FullName = driver.FullName,
                        Email = driver.Email,
                        PhoneNumber = driver.PhoneNumber,
                        DateOfBirth = driver.DateOfBirth,
                        LicenseNumber = driver.LicenseNumber,
                        LicenseImageUrl = driver.LicenseImage, // Mapping LicenseImage thành LicenseImageUrl cho DTO
                        Address = driver.Address,
                        City = driver.City,
                        District = driver.District,
                        Ward = driver.Ward,
                        IsSameAsRenter = driver.IsSameAsRenter
                    }).ToList() ?? new List<DriverInformationDto>()
                };

                var success = await _bookingListService.UpdateBookingDetailAsync(id, updateDto, userId.Value);
                
                if (success)
                {
                    return Json(new { success = true, message = "Booking information updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update booking information." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking information for ID {BookingId}", id);
                return Json(new { success = false, message = "An error occurred while updating booking information." });
            }
        }

        /// <summary>
        /// Displays booking confirmation details after successful booking completion.
        /// Shows final booking status and confirmation information for user reference.
        /// </summary>
        /// <param name="id">The unique identifier of the booking to show confirmation for.</param>
        /// <returns>The booking confirmation view or redirect if booking not found.</returns>
        [HttpGet]
        public async Task<IActionResult> Confirmation(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId() ?? Guid.Parse("EA0E2843-24B8-4769-A152-1B21FC05D3F6");

                var booking = await _bookingListService.GetBookingDetailAsync(id, userId);
                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = booking.ToEditViewModel();
                viewModel.Progress.CurrentStep = 3; // Confirmation step
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking confirmation for ID {BookingId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading booking confirmation.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Retrieves car return information including financial calculations and requirements.
        /// Provides return scenarios, refund amounts, and additional charges for user review.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item being returned.</param>
        /// <returns>JSON result with return car information and financial calculations.</returns>
        [HttpPost]
        public async Task<JsonResult> GetReturnCarInfo(Guid bookingItemId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var result = await _bookingListService.ReturnCarAsync(bookingItemId, userId.Value);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }
                var bookingDetails = await _bookingListService.GetBookingDetailAsync(bookingItemId, userId.Value);

                if (!result.RequiresWalletProcessing)
                {
                    return Json(new { 
                        success = true, 
                        requiresWalletProcessing = false,
                        message = result.Message,
                        shouldRedirectToFeedback = true,
                        bookingId = bookingDetails.BookingId,
                        carId = bookingDetails.CarInfo.Id
                    });
                }

                var scenarioType = "exact";
                if (result.AdditionalChargeNeeded > 0)
                {
                    scenarioType = result.CanProcessReturn ? "additional" : "insufficient";
                }
                else if (result.RefundAmount > 0)
                {
                    scenarioType = "refund";
                }

                return Json(new { 
                    success = true,
                    requiresWalletProcessing = true,
                    rentalAmount = result.RentalAmount,
                    depositAmount = result.DepositAmount,
                    currentWalletBalance = result.CurrentWalletBalance,
                    refundAmount = result.RefundAmount,
                    additionalChargeNeeded = result.AdditionalChargeNeeded,
                    canProcessReturn = result.CanProcessReturn,
                    scenarioType = scenarioType,
                    bookingId = bookingDetails.BookingId,
                    carId = bookingDetails.CarInfo.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting return car info for booking item {BookingItemId}", bookingItemId);
                return Json(new { success = false, message = "An error occurred while processing return information." });
            }
        }

        /// <summary>
        /// Processes the complete car return transaction including payments and status updates.
        /// Handles wallet transactions, refunds, additional charges, and booking completion.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item being returned.</param>
        /// <returns>JSON result indicating success or failure of the return transaction.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ProcessReturnCar(Guid bookingItemId)
        {
            try
            {

                var userId = GetCurrentUserId() ?? Guid.Parse("EA0E2843-24B8-4769-A152-1B21FC05D3F6");

                var success = await _bookingListService.ProcessReturnCarTransactionAsync(bookingItemId, userId);
                var bookingDetails = await _bookingListService.GetBookingDetailAsync(bookingItemId, userId);

                if (success)
                {
                    return Json(new { success = true, message = "Car returned and transactions processed successfully.",
                        bookingId = bookingDetails.BookingId,
                        carId = bookingDetails.CarInfo.Id
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to process car return transaction." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing return car transaction for booking item {BookingItemId}", bookingItemId);
                return Json(new { success = false, message = "An error occurred while processing the return transaction." });
            }
        }

        /// <summary>
        /// Updates booking item status with full page redirect for non-AJAX operations.
        /// Handles booking cancellation and pickup confirmation with user feedback.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item to update.</param>
        /// <param name="action">The action to perform (cancel, confirmpickup).</param>
        /// <returns>Redirect to booking list with success or error messages.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItemStatus(Guid bookingItemId, string action)
        {
            try
            {
                var userId = GetCurrentUserId() ?? Guid.Parse("EA0E2843-24B8-4769-A152-1B21FC05D3F6");
                bool success = false;
                string successMessage = "";

                switch (action.ToLower())
                {
                    case "cancel":
                        success = await _bookingListService.CancelBookingItemAsync(bookingItemId, userId);
                        successMessage = "Booking item has been cancelled successfully.";
                        break;
                    case "confirmpickup":
                        success = await _bookingListService.ConfirmPickupAsync(bookingItemId, userId);
                        successMessage = "Car pickup has been confirmed successfully.";
                        break;
                    default:
                        TempData["ErrorMessage"] = "Invalid action.";
                        break;
                }

                if (success)
                {
                    TempData["SuccessMessage"] = successMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update booking status.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking item status. BookingItemId: {BookingItemId}, Action: {Action}", bookingItemId, action);
                TempData["ErrorMessage"] = "An error occurred while updating booking status.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Updates booking item status via AJAX for dynamic user interface operations.
        /// Provides immediate feedback without page refresh for better user experience.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item to update.</param>
        /// <param name="action">The action to perform (cancel, confirmpickup).</param>
        /// <returns>JSON result with operation status and user-friendly messages.</returns>
        [HttpPost]
        public async Task<JsonResult> UpdateItemStatusAjax(Guid bookingItemId, string action)
        {
            try
            {
                var userId = GetCurrentUserId() ?? Guid.Parse("EA0E2843-24B8-4769-A152-1B21FC05D3F6");
                bool success = false;
                string message = "";

                switch (action.ToLower())
                {
                    case "cancel":
                        success = await _bookingListService.CancelBookingItemAsync(bookingItemId, userId);
                        message = success ? "Booking item has been cancelled successfully." : "Failed to cancel booking item.";
                        break;
                    case "confirmpickup":
                        success = await _bookingListService.ConfirmPickupAsync(bookingItemId, userId);
                        message = success ? "Car pickup has been confirmed successfully." : "Failed to confirm pickup.";
                        break;
                    default:
                        message = "Invalid action.";
                        break;
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking item status via AJAX. BookingItemId: {BookingItemId}, Action: {Action}", bookingItemId, action);
                return Json(new { success = false, message = "An error occurred while updating booking status." });
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieves the current user's unique identifier from authentication claims.
        /// Provides safe user ID extraction with proper error handling for unauthenticated users.
        /// </summary>
        /// <returns>The current user's GUID if authenticated, null otherwise.</returns>
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return null;
        }

        #endregion
    }
}
