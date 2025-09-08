using CarRental.Application.DTOs.Booking;
using CarRental.Application.Interfaces;
using CarRental.Domain.Enums;
using CarRental.MVC.Extensions;
using CarRental.MVC.Models.Booking;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CarRental.MVC.Controllers
{
    /// <summary>
    /// Handles the complete car rental booking process including multi-step form flow,
    /// session management, validation, and payment processing for customer bookings.
    /// </summary>
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;
        private readonly IPhotoService _photoService;
        private const string BOOKING_SESSION_KEY = "BookingData";

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingController"/> class.
        /// </summary>
        /// <param name="bookingService">The booking service for business logic operations.</param>
        /// <param name="logger">The logger for tracking controller operations.</param>
        /// <param name="photoService">The photo service for license image uploads.</param>
        public BookingController(
            IBookingService bookingService,
            ILogger<BookingController> logger,
            IPhotoService photoService)
        {
            _bookingService = bookingService;
            _logger = logger;
            _photoService = photoService;
        }

        #region Main Booking Flow

        /// <summary>
        /// Displays the booking process interface and manages multi-step booking flow.
        /// Handles new booking initialization and existing session restoration.
        /// </summary>
        /// <param name="carIds">Comma-separated list of car IDs to book.</param>
        /// <param name="pickupDate">The intended pickup date for the rental.</param>
        /// <param name="returnDate">The intended return date for the rental.</param>
        /// <param name="step">The current step in the booking process (1-3).</param>
        /// <param name="isNewSession">Flag indicating if a new session should be created.</param>
        /// <returns>The booking view with appropriate step content.</returns>
        [HttpPost]
        public async Task<IActionResult> Index(string? carIds = null, DateTime? pickupDate = null, DateTime? returnDate = null, int step = 1, bool isNewSession = false)
        {
            // Get BookingData from session if have 
            //var bookingData = GetBookingDataFromSession();
            BookingViewModel? bookingData = isNewSession ? null : GetBookingDataFromSession();

            // If no session data initialize new booking view
            if (bookingData == null && !string.IsNullOrEmpty(carIds))
            {
                bookingData = await InitializeNewBookingAsync(carIds, pickupDate, returnDate);
                SaveBookingDataToSession(bookingData);
            }

            // If bookingData is null then redirect to Car Index 
            if (bookingData == null)
            {
                TempData["ErrorMessage"] = "Please select cars to book first";
                return RedirectToAction("Index", "Car");
            }

            step = Math.Max(1, Math.Min(2, step));
            bookingData.Progress.CurrentStep = step;
            bookingData.Progress.TotalSteps = 3;

            return View(bookingData);
        }

        /// <summary>
        /// Processes booking information from step 1 and advances to payment step.
        /// Handles license image uploads, validation, and session data updates.
        /// </summary>
        /// <param name="information">The booking information collected from the user.</param>
        /// <returns>JSON result indicating success or validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContinueToPayment(BookingInformationFormViewModel information)
        {
            var bookingData = GetBookingDataFromSession();
            if (bookingData == null)
            {
                return Json(new { success = false, message = "Booking session has expired. Please start over." });
            }

            // Handle license image uploads và lưu vào LicenseImage field
            try
            {
                // Upload renter license image nếu có file
                if (information.Renter.LicenseImageFile != null && information.Renter.LicenseImageFile.Length > 0)
                {
                    var renterResult = await _photoService.AddPhotoAsync(information.Renter.LicenseImageFile);
                    if (renterResult.Error == null)
                    {
                        information.Renter.LicenseImage = renterResult.SecureUrl.AbsoluteUri;
                    }
                }

                // Upload driver license images nếu có file
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading license images during booking");
                // Tiếp tục mà không fail toàn bộ process
            }

            // Clear IFormFile objects sau khi đã upload
            information.Renter.LicenseImageFile = null;
            foreach (var driver in information.Drivers)
            {
                driver.LicenseImageFile = null;
            }

            // Save information to session
            bookingData.Information = information;
            bookingData.PickupDate = information.PickupDate;
            bookingData.ReturnDate = information.ReturnDate;
            bookingData.NumberOfDays = CalculateNumberOfDays(information.PickupDate, information.ReturnDate);

            // Update step to payment
            bookingData.Progress.CurrentStep = 2;

            // Recalculate prices and refresh payment methods
            await RecaculatePricesAsync(bookingData);
            await RefreshPaymentMethodAsync(bookingData);

            SaveBookingDataToSession(bookingData);

            return Json(new { 
                success = true, 
                message = "Information saved. Proceeding to payment...",
                redirectToStep = 2,
                data = new
                {
                    numberOfDays = bookingData.NumberOfDays,
                    totalAmount = bookingData.TotalAmount,
                    totalDeposit = bookingData.TotalDeposit,
                    carItems = bookingData.CarItems.Select(c => new
                    {
                        carId = c.CarId,
                        carName = c.CarName,
                        pricePerDay = c.PricePerDay,
                        subTotal = c.SubTotal,
                        deposit = c.Deposit,
                    }),
                    payment = new
                    {
                        userWallet = new
                        {
                            balance = bookingData.Payment.UserWallet.Balance,
                            hasWallet = bookingData.Payment.UserWallet.HasWallet
                        },
                        paymentMethods = new
                        {
                            wallet = new
                            {
                                isAvailable = bookingData.Payment.PaymentMethods.Wallet.IsAvailable,
                                currentBalance = bookingData.Payment.PaymentMethods.Wallet.CurrentBalance,
                                requiredAmount = bookingData.Payment.PaymentMethods.Wallet.RequiredAmount,
                                hasSufficientFunds = bookingData.Payment.PaymentMethods.Wallet.HasSufficientFunds
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Processes payment information and completes the booking transaction.
        /// Validates all booking data and executes payment processing based on selected method.
        /// </summary>
        /// <param name="payment">The payment information and method selection.</param>
        /// <returns>JSON result with booking confirmation data or error messages.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(BookingPaymentFormViewModel payment)
        {
            var bookingData = GetBookingDataFromSession();
            if (bookingData == null)
            {
                return Json(new { success = false, message = "Booking session has expired. Please start over." });
            }

            // Validate ALL data now (both step 1 and step 2)
            var overallValidation = await ValidateAllStepsAsync(bookingData, payment);
            if (!overallValidation.IsValid)
            {
                // Find the first invalid step and return detailed errors
                var firstErrorStep = overallValidation.Errors.Min(e => e.Step);
                var step1Errors = overallValidation.Errors.Where(e => e.Step == 1).ToList();
                var step2Errors = overallValidation.Errors.Where(e => e.Step == 2).ToList();

                return Json(new
                {
                    success = false,
                    message = "Please correct the following errors before proceeding",
                    redirectToStep = firstErrorStep,
                    errors = overallValidation.Errors,
                    step1Errors,
                    step2Errors,
                    focusField = overallValidation.Errors.First().Field
                });
            }

            // Process the actual booking
            var userId = GetCurrentUserId() ?? Guid.Parse("EA0E2843-24B8-4769-A152-1B21FC05D3F6");
                
            // Map ViewModel to DTO
            var bookingProcessDto = bookingData.ToBookingProcessDto();
            var paymentMethodType = payment.SelectedPaymentMethod;
                
            var bookingResult = await _bookingService.ProcessBookingAsync(
                bookingProcessDto, paymentMethodType, userId);

            if (!bookingResult.IsSuccess)
            {
                if (bookingResult.ValidationErrors.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = "Validation failed. Please check your information.",
                        errors = bookingResult.ValidationErrors 
                    });
                }
                    
                return Json(new { success = false, message = bookingResult.ErrorMessage });
            }

            // Update confirmation data
            bookingData.Confirmation = new BookingConfirmationViewModel
            {
                BookingNumber = bookingResult.BookingNumber,
                BookingDate = DateTime.Now,
                BookingStatus = paymentMethodType == PaymentMethodTypeEnum.Wallet ? "Confirmed" : "Pending", // Add BookingStatus
                PaymentMethod = GetPaymentMethodName(payment.SelectedPaymentMethod),
                PaymentStatus = paymentMethodType == PaymentMethodTypeEnum.Wallet ? "Paid" : "Pending",
                NextSteps = GetNextStepsForPaymentMethod(payment.SelectedPaymentMethod)
            };

            ClearBookingSession();
                
            return Json(new
            {
                success = true,
                message = "Booking completed successfully!",
                data = bookingData.Confirmation
            });
        }

        /// <summary>
        /// Clears the current booking session data from server storage.
        /// Used for cleanup after successful booking completion or user cancellation.
        /// </summary>
        /// <returns>JSON result confirming session clearance.</returns>
        [HttpPost]
        public IActionResult ClearBookingSession()
        {
            HttpContext.Session.Remove(BOOKING_SESSION_KEY);
            return Json(new { success = true });
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Retrieves booking data from the current user session.
        /// Handles deserialization errors gracefully by clearing corrupted session data.
        /// </summary>
        /// <returns>The booking view model if session exists, null otherwise.</returns>
        private BookingViewModel? GetBookingDataFromSession()
        {
            var sessionData = HttpContext.Session.GetString(BOOKING_SESSION_KEY);
            if (string.IsNullOrEmpty(sessionData))
                return null;

            try
            {
                return JsonSerializer.Deserialize<BookingViewModel>(sessionData);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize booking session data");
                HttpContext.Session.Remove(BOOKING_SESSION_KEY);
                return null;
            }
        }

        /// <summary>
        /// Saves booking data to the user session for persistence across requests.
        /// Serializes booking data while excluding file upload properties for storage efficiency.
        /// </summary>
        /// <param name="bookingData">The booking data to store in session.</param>
        private void SaveBookingDataToSession(BookingViewModel bookingData)
        {
            try
            {
                // JsonIgnore sẽ tự động bỏ qua LicenseImageFile properties
                var sessionData = JsonSerializer.Serialize(bookingData);
                HttpContext.Session.SetString(BOOKING_SESSION_KEY, sessionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save booking data to session");
            }
        }

        /// <summary>
        /// Initializes a new booking session with selected cars and date information.
        /// Validates car availability, calculates pricing, and prepares booking structure.
        /// </summary>
        /// <param name="carIds">Comma-separated list of car IDs for the booking.</param>
        /// <param name="pickupDate">The intended pickup date for the rental.</param>
        /// <param name="returnDate">The intended return date for the rental.</param>
        /// <returns>A fully initialized booking view model with car and pricing information.</returns>
        private async Task<BookingViewModel> InitializeNewBookingAsync(string carIds, DateTime? pickupDate, DateTime? returnDate)
        {
            var carIdList = carIds.Split(',').Select(id => Guid.Parse(id)).ToList();
            var pickup = pickupDate ?? DateTime.Today.AddDays(1);
            var returnDt = returnDate ?? DateTime.Today.AddDays(2);

            // Check car availability
            var available = await _bookingService.AreCarsAvailableAsync(carIdList, pickup, returnDt);
            if (!available)
            {
                throw new InvalidOperationException("One or more selected cars are not available for the specified dates.");
            }

            // Get car information
            var cars = await _bookingService.GetCarInformationAsync(carIdList);
            var carItems = cars.Select(car => new CarSummaryItem
            {
                CarId = car.Id,
                CarName = $"{car.Brand} {car.Model}",
                LicensePlate = car.LicensePlate,
                Location = car.Location,
                PricePerDay = car.PricePerDay,
                Deposit = car.RequiredDeposit,
                Seats = car.Seats,
                AverageRating = car.AverageRating,
                Transmission = car.Transmission,
                FuelType = car.FuelType,
                TotalTrips = car.TotalTrips,
                ImagePath = car.ImageUrl,
            }).ToList();

            var numberOfDays = CalculateNumberOfDays(pickup, returnDt);

            // Calculate totals
            foreach (var item in carItems)
            {
                item.SubTotal = item.PricePerDay * numberOfDays;
            }

            var bookingData = new BookingViewModel
            {
                SessionId = Guid.NewGuid().ToString(),
                SelectedCarIds = carIdList,
                PickupDate = pickup,
                ReturnDate = returnDt,
                NumberOfDays = numberOfDays,
                CarItems = carItems,
                TotalAmount = carItems.Sum(c => c.SubTotal),
                TotalDeposit = carItems.Sum(c => c.Deposit),
                Progress = new BookingProgressViewModel
                {
                    CurrentStep = 1,
                    TotalSteps = 3
                },
                Information = new BookingInformationFormViewModel
                {
                    PickupDate = pickup,
                    ReturnDate = returnDt,
                    Drivers = carItems.Select(car => new DriverInformationViewModel
                    {
                        CarId = car.CarId,
                        CarName = car.CarName,
                        IsSameAsRenter = false
                    }).ToList()
                }
            };

            // Load user data if user is logged in
            await LoadUserDataAsync(bookingData);

            // Initialize payment methods
            var userId = GetCurrentUserId() ?? Guid.Parse("EA0E2843-24B8-4769-A152-1B21FC05D3F6");
            var userWallet = await _bookingService.GetUserWalletAsync(userId);

            bookingData.Payment = new BookingPaymentFormViewModel
            {
                UserWallet = new WalletInformationViewModel
                {
                    Balance = userWallet.Balance,
                    HasWallet = userWallet.HasWallet
                },
                PaymentMethods = new PaymentMethodsViewModel
                {
                    Wallet = new WalletPaymentMethodViewModel
                    {
                        IsAvailable = userWallet.HasWallet,
                        CurrentBalance = userWallet.Balance,
                        RequiredAmount = bookingData.TotalDeposit,
                    },
                    Cash = new CashPaymentMethodViewModel
                    {
                        IsAvailable = true
                    },
                    BankTransfer = new BankTransferPaymentMethodViewModel
                    {
                        IsAvailable = true
                    }
                }
            };

            return bookingData;
        }

        /// <summary>
        /// Loads user profile data into the booking model for form pre-population.
        /// Retrieves personal information, license details, and address data from user account.
        /// </summary>
        /// <param name="bookingData">The booking data to populate with user information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadUserDataAsync(BookingViewModel bookingData)
        {
            Guid?  userId = GetCurrentUserId() ?? Guid.Parse("EA0E2843-24B8-4769-A152-1B21FC05D3F6");
            if (userId.HasValue)
            {
                // Use BookingService to get user profile instead of UserService
                var renterInfo = await _bookingService.GetUserProfileAsync(userId.Value);
                    
                if (renterInfo != null)
                {
                    bookingData.Information.Renter.FullName = renterInfo.FullName;
                    bookingData.Information.Renter.Email = renterInfo.Email;
                    bookingData.Information.Renter.PhoneNumber = renterInfo.PhoneNumber;
                    bookingData.Information.Renter.Address = renterInfo.Address;
                    bookingData.Information.Renter.City = renterInfo.City;
                    bookingData.Information.Renter.District = renterInfo.District;
                    bookingData.Information.Renter.Ward = renterInfo.Ward; // Added Ward field
                    bookingData.Information.Renter.DateOfBirth = renterInfo.DateOfBirth;
                    bookingData.Information.Renter.LicenseNumber = renterInfo.LicenseNumber;
                    bookingData.Information.Renter.LicenseImage = renterInfo.LicenseImageUrl; // Sử dụng LicenseImage thay vì LicenseImageUrl
                }
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates booking information including dates, renter details, and driver information.
        /// Performs comprehensive validation including age verification and license conflict checking.
        /// </summary>
        /// <param name="information">The booking information to validate.</param>
        /// <param name="bookingData">The complete booking context for validation.</param>
        /// <returns>A validation result containing any errors found during validation.</returns>
        private async Task<ValidationResult> ValidateInformationStepAsync(BookingInformationFormViewModel information, BookingViewModel bookingData)
        {
            var result = new ValidationResult();

            // Date validation
            if (information.PickupDate < DateTime.Today)
            {
                result.AddError("PickupDate", "Pickup date cannot be in the past", 1);
            }

            if (information.ReturnDate <= information.PickupDate)
            {
                result.AddError("ReturnDate", "Return date must be after pickup date", 1);
            }

            // Renter validation
            if (string.IsNullOrWhiteSpace(information.Renter.FullName))
            {
                result.AddError("Renter.FullName", "Renter full name is required", 1);
            }

            if (string.IsNullOrWhiteSpace(information.Renter.Email) || !IsValidEmail(information.Renter.Email))
            {
                result.AddError("Renter.Email", "Valid email address is required", 1);
            }

            if (string.IsNullOrWhiteSpace(information.Renter.PhoneNumber))
            {
                result.AddError("Renter.PhoneNumber", "Phone number is required", 1);
            }

            if (string.IsNullOrWhiteSpace(information.Renter.LicenseNumber))
            {
                result.AddError("Renter.LicenseNumber", "Driver license number is required", 1);
            }

            if (string.IsNullOrWhiteSpace(information.Renter.Address))
            {
                result.AddError("Renter.Address", "Address is required", 1);
            }

            var age = DateTime.Today.Year - information.Renter.DateOfBirth.Year;
            if (information.Renter.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 18)
            {
                result.AddError("Renter.DateOfBirth", "Renter must be at least 18 years old", 1);
            }

            // Driver validation
            for (int i = 0; i < information.Drivers.Count; i++)
            {
                var driver = information.Drivers[i];
                if (!driver.IsSameAsRenter)
                {
                    if (string.IsNullOrWhiteSpace(driver.FullName))
                    {
                        result.AddError($"Drivers[{i}].FullName", $"Driver full name is required for {driver.CarName}", 1);
                    }

                    if (string.IsNullOrWhiteSpace(driver.LicenseNumber))
                    {
                        result.AddError($"Drivers[{i}].LicenseNumber", $"Driver license number is required for {driver.CarName}", 1);
                    }

                    if (string.IsNullOrWhiteSpace(driver.PhoneNumber))
                    {
                        result.AddError($"Drivers[{i}].PhoneNumber", $"Driver phone number is required for {driver.CarName}", 1);
                    }

                    if (string.IsNullOrWhiteSpace(driver.Email) || !IsValidEmail(driver.Email))
                    {
                        result.AddError($"Drivers[{i}].Email", $"Valid driver email is required for {driver.CarName}", 1);
                    }

                    if (string.IsNullOrWhiteSpace(driver.Address))
                    {
                        result.AddError($"Drivers[{i}].Address", $"Driver address is required for {driver.CarName}", 1);
                    }

                    // Age validation for driver
                    var driverAge = DateTime.Today.Year - driver.DateOfBirth.Year;
                    if (driver.DateOfBirth.Date > DateTime.Today.AddYears(-driverAge)) driverAge--;

                    if (driverAge < 18)
                    {
                        result.AddError($"Drivers[{i}].DateOfBirth", $"Driver must be at least 18 years old for {driver.CarName}", 1);
                    }
                }
            }

            // Check for duplicate license numbers
            var licenseNumbers = information.Drivers
                .Where(d => !string.IsNullOrWhiteSpace(d.LicenseNumber) && !d.IsSameAsRenter)
                .Select(d => d.LicenseNumber.Trim().ToUpper())
                .ToList();

            if (!string.IsNullOrWhiteSpace(information.Renter.LicenseNumber))
            {
                licenseNumbers.Add(information.Renter.LicenseNumber.Trim().ToUpper());
            }

            var duplicates = licenseNumbers.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicates.Any())
            {
                result.AddError("Drivers", "Duplicate license numbers are not allowed", 1);
            }

            // Check license availability
            var hasOverlapping = await _bookingService.CheckLicenseOverlappingAsync(
                information.Drivers.Select(d => new DriverInformationDto
                {
                    LicenseNumber = d.IsSameAsRenter ? information.Renter.LicenseNumber : d.LicenseNumber,
                    CarId = d.CarId
                }).ToList(),
                information.PickupDate,
                information.ReturnDate);

            if (hasOverlapping)
            {
                result.AddError("Drivers", "One or more driver licenses are already booked during this period", 1);
            }

            return result;
        }

        /// <summary>
        /// Validates all booking steps comprehensively before final processing.
        /// Re-validates information step and validates payment selection and requirements.
        /// </summary>
        /// <param name="bookingData">The complete booking data to validate.</param>
        /// <param name="payment">The payment information to validate.</param>
        /// <returns>A validation result with errors from all validation steps.</returns>
        private async Task<ValidationResult> ValidateAllStepsAsync(BookingViewModel bookingData, BookingPaymentFormViewModel payment)
        {
            var result = new ValidationResult();

            // Re-validate step 1
            var step1Validation = await ValidateInformationStepAsync(bookingData.Information, bookingData);
            result.Errors.AddRange(step1Validation.Errors);

            // Validate payment step
            if (payment.SelectedPaymentMethod == 0)
            {
                result.AddError("PaymentMethod", "Please select a payment method", 2);
            }

            // Validate wallet balance if wallet selected
            if (payment.SelectedPaymentMethod == PaymentMethodTypeEnum.Wallet)
            {
                if (!bookingData.Payment.PaymentMethods.Wallet.HasSufficientFunds)
                {
                    result.AddError("PaymentMethod", "Insufficient wallet balance. Please choose a different payment method or top up your wallet.", 2);
                }
            }

            var available = await _bookingService.AreCarsAvailableAsync(
                bookingData.SelectedCarIds,
                bookingData.Information.PickupDate,
                bookingData.Information.ReturnDate);

            if (!available)
            {
                result.AddError("CarAvailability", "One or more cars are no longer available for the selected dates. Please choose different dates or cars.", 1);
            }

            return result;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculates the number of rental days between pickup and return dates.
        /// Ensures minimum of 1 day for pricing calculations.
        /// </summary>
        /// <param name="pickupDate">The rental start date.</param>
        /// <param name="returnDate">The rental end date.</param>
        /// <returns>The number of rental days as an integer.</returns>
        private decimal CalculateNumberOfDays(DateTime pickupDate, DateTime returnDate)
        {
            var totalHours = (returnDate - pickupDate).TotalHours; // double
            if (totalHours < 0) totalHours = 0; // tránh trường hợp returnDate < pickupDate
            return Math.Round((decimal)totalHours/24, 1); // làm tròn 1 số thập phân
        }

        /// <summary>
        /// Validates email address format using standard email validation.
        /// Provides reliable email format checking for user input validation.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if email format is valid, false otherwise.</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts payment method enum to user-friendly display name.
        /// Provides localized payment method names for user interface display.
        /// </summary>
        /// <param name="paymentMethod">The payment method enum to convert.</param>
        /// <returns>A human-readable payment method name.</returns>
        private string GetPaymentMethodName(PaymentMethodTypeEnum paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodTypeEnum.Wallet => "Digital Wallet",
                PaymentMethodTypeEnum.Cash => "Cash Payment",
                PaymentMethodTypeEnum.BankTransfer => "Bank Transfer",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Generates appropriate next steps based on the selected payment method.
        /// Provides user guidance on actions required after booking completion.
        /// </summary>
        /// <param name="paymentMethod">The payment method used for the booking.</param>
        /// <returns>A list of next step instructions for the user.</returns>
        private List<string> GetNextStepsForPaymentMethod(PaymentMethodTypeEnum paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodTypeEnum.Wallet =>
                [
                    "Your booking has been confirmed and deposit has been deducted from your wallet",
                    "We will contact you 1 day before pickup to confirm details",
                    "Please bring your ID and driver's license when picking up the car",
                    "📧 Vehicle pickup location details have been sent to your email",
                    "📱 You can view your booking status anytime in 'My Bookings' section"
                ],
                PaymentMethodTypeEnum.Cash or PaymentMethodTypeEnum.BankTransfer =>
                [
                    "Please bring cash deposit when picking up the car",
                    "Deposit amount required: " + "{{DEPOSIT_AMOUNT}}" + " VND (exact amount preferred)",
                    "We will contact you 1 day before pickup to confirm details and payment",
                    "Please bring your ID and driver's license when picking up the car",
                    "Payment instructions have been sent to your email",
                    "Your booking will be confirmed after deposit payment"
                ],
                _ => []
            };
        }

        /// <summary>
        /// Recalculates booking pricing based on updated date information.
        /// Updates car subtotals and booking totals when dates are modified.
        /// </summary>
        /// <param name="bookingViewModel">The booking data to recalculate pricing for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RecaculatePricesAsync(BookingViewModel bookingViewModel)
        {
            foreach (var item in bookingViewModel.CarItems)
            {
                item.SubTotal = item.PricePerDay * bookingViewModel.NumberOfDays;
            }

            bookingViewModel.TotalAmount = bookingViewModel.CarItems.Sum(c => c.SubTotal);
            bookingViewModel.TotalDeposit = bookingViewModel.CarItems.Sum(c => c.Deposit);
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Refreshes payment method availability and wallet balance information.
        /// Updates payment options based on current user wallet status and booking requirements.
        /// </summary>
        /// <param name="bookingViewModel">The booking data to update payment information for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshPaymentMethodAsync(BookingViewModel bookingViewModel)
        {
            var userId = GetCurrentUserId();
            var userWallet = await _bookingService.GetUserWalletAsync(userId.Value);
            
            bookingViewModel.Payment.UserWallet.Balance = userWallet.Balance;
            bookingViewModel.Payment.UserWallet.HasWallet = userWallet.HasWallet;
            bookingViewModel.Payment.PaymentMethods.Wallet = new WalletPaymentMethodViewModel
            {
                IsAvailable = userWallet.HasWallet,
                CurrentBalance = userWallet.Balance,
                RequiredAmount = bookingViewModel.TotalDeposit,
            };

            await Task.CompletedTask;

        }

        /// <summary>
        /// Retrieves the current user's unique identifier from authentication claims.
        /// Provides safe user ID extraction with null handling for unauthenticated users.
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

    #region Helper Classes

    /// <summary>
    /// Represents the result of a validation operation with detailed error information.
    /// Provides comprehensive validation feedback for form processing and user guidance.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets the list of validation errors found during validation.
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new();

        /// <summary>
        /// Gets a value indicating whether the validation was successful (no errors found).
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds a validation error to the result with field and step information.
        /// </summary>
        /// <param name="field">The field name that failed validation.</param>
        /// <param name="message">The error message describing the validation failure.</param>
        /// <param name="step">The booking step where the error occurred.</param>
        public void AddError(string field, string message, int step)
        {
            Errors.Add(new ValidationError { Field = field, Message = message, Step = step });
        }
    }

    /// <summary>
    /// Represents a single validation error with field, message, and step information.
    /// Provides detailed error context for user interface error display and field highlighting.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets or sets the field name that failed validation.
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message describing the validation failure.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the booking step where the validation error occurred.
        /// </summary>
        public int Step { get; set; }
    }

    #endregion
}