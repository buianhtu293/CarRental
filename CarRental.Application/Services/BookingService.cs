using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using CarRental.Application.Interfaces;
using Microsoft.Extensions.Logging;
using CarRental.Domain.Interfaces;
using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Enums;

namespace CarRental.Application.Services
{
    /// <summary>
    /// Provides comprehensive booking management services for car rental operations.
    /// Handles the complete booking lifecycle from creation to completion including payment processing,
    /// validation, and session management.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BookingService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingService"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work for data access operations.</param>
        /// <param name="userManager">The user manager for user-related operations.</param>
        /// <param name="cache">The memory cache for session management.</param>
        /// <param name="logger">The logger for tracking service operations.</param>
        public BookingService(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IMemoryCache cache,
            ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        #region Session Management

        /// <summary>
        /// Creates a new booking session for the specified cars and date range.
        /// Initializes booking data and stores it in cache for temporary persistence.
        /// </summary>
        /// <param name="carIds">The list of car IDs to be included in the booking.</param>
        /// <param name="pickupDate">The intended pickup date for the rental.</param>
        /// <param name="returnDate">The intended return date for the rental.</param>
        /// <returns>A unique session identifier for tracking the booking process.</returns>
        public async Task<string> CreateBookingSessionAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate)
        {
            var sessionId = Guid.NewGuid().ToString();
            var bookingData = await InitializeBookingInformationAsync(carIds, pickupDate, returnDate);
            bookingData.BookingSessionId = sessionId;
            
            _cache.Set($"booking_session_{sessionId}", bookingData, TimeSpan.FromHours(2));
            
            _logger.LogInformation("Created booking session {SessionId} for cars {CarIds}", sessionId, string.Join(",", carIds));
            return sessionId;
        }

        /// <summary>
        /// Retrieves booking data from the active session cache.
        /// Returns null if the session has expired or doesn't exist.
        /// </summary>
        /// <param name="sessionId">The unique session identifier.</param>
        /// <returns>The booking information if session exists, otherwise null.</returns>
        public Task<BookingInformationDto?> GetBookingSessionAsync(string sessionId)
        {
            _cache.TryGetValue($"booking_session_{sessionId}", out BookingInformationDto? bookingData);
            return Task.FromResult(bookingData);
        }

        /// <summary>
        /// Updates the booking session cache with modified booking information.
        /// Extends the session expiration time for continued user activity.
        /// </summary>
        /// <param name="sessionId">The unique session identifier to update.</param>
        /// <param name="model">The updated booking information to store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateBookingSessionAsync(string sessionId, BookingInformationDto model)
        {
            model.BookingSessionId = sessionId;
            _cache.Set($"booking_session_{sessionId}", model, TimeSpan.FromHours(2));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes booking data from the session cache.
        /// Typically called after successful booking completion or session timeout.
        /// </summary>
        /// <param name="sessionId">The session identifier to clear.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ClearBookingSessionAsync(string sessionId)
        {
            _cache.Remove($"booking_session_{sessionId}");
            return Task.CompletedTask;
        }

        #endregion

        #region Step 1: Booking Information

        /// <summary>
        /// Initializes a new booking with car information, pricing calculations, and user data.
        /// Prepares all necessary data structures for the booking information step.
        /// </summary>
        /// <param name="carIds">The list of car IDs selected for booking.</param>
        /// <param name="pickupDate">The intended pickup date for the rental.</param>
        /// <param name="returnDate">The intended return date for the rental.</param>
        /// <param name="userId">Optional user ID to pre-populate user information.</param>
        /// <returns>A complete booking information DTO with car details and pricing.</returns>
        public async Task<BookingInformationDto> InitializeBookingInformationAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate, Guid? userId = null)
        {
            var cars = await GetCarInformationAsync(carIds);
            var numberOfDays = await CalculateNumberOfDaysAsync(pickupDate, returnDate);
            var totalAmount = await CalculateTotalAmountAsync(carIds, pickupDate, returnDate);
            var totalDeposit = await CalculateTotalDepositAsync(carIds);

            var model = new BookingInformationDto
            {
                Progress = new BookingProgressDto { CurrentStep = 1 },
                SelectedCarIds = carIds,
                PickupDate = pickupDate,
                ReturnDate = returnDate,
                Summary = new BookingSummaryDto
                {
                    NumberOfDays = numberOfDays,
                    TotalAmount = totalAmount,
                    TotalDeposit = totalDeposit,
                    PickupDate = pickupDate,
                    ReturnDate = returnDate,
                    NumberOfCars = carIds.Count,
                    CarItems = cars.Select(c => new CarSummaryItemDto
                    {
                        CarId = c.Id,
                        CarName = $"{c.Brand} {c.Model}",
                        PricePerDay = c.PricePerDay,
                        Deposit = c.RequiredDeposit,
                        AverageRating = c.AverageRating,
                        FuelType = c.FuelType,
                        LicensePlate = c.LicensePlate,
                        Location = c.Location,
                        Seats = c.Seats,
                        Transmission = c.Transmission,
                        TotalTrips = c.TotalTrips,
                        SubTotal = c.PricePerDay * numberOfDays
                    }).ToList()
                }
            };

            // Initialize driver information for each car
            model.Drivers = cars.Select(car => new DriverInformationDto
            {
                CarId = car.Id,
                CarName = $"{car.Brand} {car.Model}",
                IsSameAsRenter = false
            }).ToList();

            // Load user profile if available
            if (userId.HasValue)
            {
                model.Renter = await GetUserProfileAsync(userId.Value);
            }

            return model;
        }

        /// <summary>
        /// Validates all booking information including dates, user details, and driver information.
        /// Performs comprehensive validation including car availability and license overlap checking.
        /// </summary>
        /// <param name="model">The booking information to validate.</param>
        /// <returns>A validation result containing any errors found during validation.</returns>
        public async Task<ValidationResult> ValidateBookingInformationAsync(BookingInformationDto model)
        {
            var result = new ValidationResult();

            // Validate basic model state
            if (model.PickupDate <= DateTime.Today)
            {
                result.AddError("PickupDate", "Pickup date must be after today.");
            }

            if (model.ReturnDate <= model.PickupDate)
            {
                result.AddError("ReturnDate", "Return date must be after pickup date.");
            }

            // Validate renter information
            if (string.IsNullOrWhiteSpace(model.Renter.FullName))
            {
                result.AddError("Renter.FullName", "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Renter.Email))
            {
                result.AddError("Renter.Email", "Email is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Renter.PhoneNumber))
            {
                result.AddError("Renter.PhoneNumber", "Phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Renter.LicenseNumber))
            {
                result.AddError("Renter.LicenseNumber", "License number is required.");
            }

            // Validate driver information
            for (int i = 0; i < model.Drivers.Count; i++)
            {
                var driver = model.Drivers[i];
                var carName = driver.CarName;

                if (!driver.IsSameAsRenter)
                {
                    if (string.IsNullOrWhiteSpace(driver.FullName))
                    {
                        result.AddError($"Drivers[{i}].FullName", $"Driver's full name for {carName} is required.");
                    }

                    if (string.IsNullOrWhiteSpace(driver.LicenseNumber))
                    {
                        result.AddError($"Drivers[{i}].LicenseNumber", $"Driver's license number for {carName} is required.");
                    }

                    if (driver.DateOfBirth == default)
                    {
                        result.AddError($"Drivers[{i}].DateOfBirth", $"Driver's date of birth for {carName} can be defaultt.");
                    }
                }
            }

            // Check car availability
            var unavailableCars = await GetUnavailableCarsAsync(model.SelectedCarIds, model.PickupDate, model.ReturnDate);
            if (unavailableCars.Any())
            {
                foreach (var car in unavailableCars)
                {
                    result.AddError("CarAvailability", $"Car {car.Brand} {car.Model} (License plate: {car.LicensePlate}) is already booked during this period.");
                }
            }

            // Check license overlapping
            var overlappingLicenses = await GetOverlappingLicensesAsync(model.Drivers, model.PickupDate, model.ReturnDate);
            if (overlappingLicenses.Any())
            {
                foreach (var driver in overlappingLicenses)
                {
                    result.AddError("LicenseOverlapping", $"License number {driver.LicenseNumber} is already being used for another booking during this period (car: {driver.CarName}).");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates payment method selection and ensures sufficient funds for wallet payments.
        /// Checks payment method availability and user wallet balance when applicable.
        /// </summary>
        /// <param name="model">The payment information to validate.</param>
        /// <returns>A validation result indicating payment validity.</returns>
        public async Task<ValidationResult> ValidatePaymentAsync(PaymentDto model)
        {
            var result = new ValidationResult();

            if (model.SelectedPaymentMethod == PaymentMethodTypeEnum.Wallet)
            {
                if (!model.PaymentMethods.Wallet.IsAvailable)
                {
                    result.AddError("SelectedPaymentMethod", "Digital wallet is not available.");
                }
                else if (!model.PaymentMethods.Wallet.HasSufficientFunds)
                {
                    var required = model.PaymentMethods.Wallet.RequiredAmount;
                    var current = model.PaymentMethods.Wallet.CurrentBalance;
                    var shortage = required - current;
                    result.AddError("SelectedPaymentMethod", $"Insufficient wallet balance. You need an additional {shortage:N0} VND to complete the payment.");
                }
            }

            return result;
        }

        #endregion

        #region Step 2: Payment

        /// <summary>
        /// Initializes payment information including available payment methods and user wallet details.
        /// Prepares payment options based on user wallet status and booking requirements.
        /// </summary>
        /// <param name="sessionId">The active booking session identifier.</param>
        /// <param name="userId">The user ID for wallet and payment information retrieval.</param>
        /// <returns>A payment DTO with available payment methods and requirements.</returns>
        public async Task<PaymentDto> InitializePaymentAsync(string sessionId, Guid userId)
        {
            var bookingData = await GetBookingSessionAsync(sessionId);
            if (bookingData == null)
                throw new InvalidOperationException("Booking session not found");

            var userWallet = await GetUserWalletAsync(userId);
            var totalAmount = bookingData.Summary.TotalDeposit;

            var model = new PaymentDto
            {
                Progress = new BookingProgressDto { CurrentStep = 2 },
                BookingSessionId = sessionId,
                BookingSummary = bookingData.Summary,
                UserWallet = userWallet,
                PaymentMethods = new PaymentMethodsDto
                {
                    Wallet = new WalletPaymentMethodDto
                    {
                        IsAvailable = userWallet.HasWallet,
                        CurrentBalance = userWallet.Balance,
                        RequiredAmount = totalAmount
                    },
                    Cash = new CashPaymentMethodDto(),
                    BankTransfer = new BankTransferPaymentMethodDto()
                }
            };

            return model;
        }

        #endregion

        #region Step 3: Finish Booking

        /// <summary>
        /// Processes the complete booking including database persistence and payment execution.
        /// Creates booking entities, processes payments, and generates confirmation details.
        /// </summary>
        /// <param name="sessionId">The booking session identifier containing booking data.</param>
        /// <param name="paymentMethod">The selected payment method for the booking.</param>
        /// <param name="userId">The user ID creating the booking.</param>
        /// <returns>A booking finish DTO with confirmation details and next steps.</returns>
        public async Task<BookingFinishDto> ProcessBookingAsync(string sessionId, PaymentMethodTypeEnum paymentMethod, Guid userId)
        {
            var bookingData = await GetBookingSessionAsync(sessionId);
            if (bookingData == null)
                throw new InvalidOperationException("Booking session not found");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Generate booking number
                var bookingNumber = await GenerateBookingNumberAsync();

                // Create booking
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    BookingNo = bookingNumber,
                    RenterID = userId,
                    PickupDate = bookingData.PickupDate,
                    ReturnDate = bookingData.ReturnDate,
                    TotalAmount = bookingData.Summary.TotalAmount,
                    TransactionType = paymentMethod.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Bookings.AddAsync(booking);

                // Create booking items for each car
                foreach (var (item, index) in bookingData.Summary.CarItems.Select((item, index) => ( item, index)))
                {
                    var driver = bookingData.Drivers[index];
                    var car = await _unitOfWork.Cars.GetByIdAsync(item.CarId);

                    if (car == null) continue;

                    var bookingItem = new BookingItem
                    {
                        Id = Guid.NewGuid(),
                        BookingID = booking.Id,
                        CarID = item.CarId,
                        PricePerDay = car.BasePricePerDay,
                        Deposit = car.RequiredDeposit,
                        LicenseID = driver.IsSameAsRenter ? bookingData.Renter.LicenseNumber : driver.LicenseNumber,
                        LicenseImage = "", // Would be uploaded separately
                        Status = paymentMethod.Equals(PaymentMethodTypeEnum.Wallet) ? BookingItemStatusEnum.Confirm : BookingItemStatusEnum.PendingDeposit,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.BookingItems.AddAsync(bookingItem);
                }

                // Process payment
                if (paymentMethod == PaymentMethodTypeEnum.Wallet)
                {
                    var success = await _unitOfWork.Wallets.DeductBalanceAsync(userId, bookingData.Summary.TotalDeposit);
                    if (!success)
                    {
                        throw new InvalidOperationException("Insufficient wallet balance");
                    }

                    // Create WalletEntry record for PayDeposit transaction
                    var userWallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
                    if (userWallet != null)
                    {
                        var walletEntry = new WalletEntry
                        {
                            Id = Guid.NewGuid(),
                            WalletId = userWallet.Id,
                            Amount = -bookingData.Summary.TotalDeposit,
                            Type = WalletEntryType.PayDeposit,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Repository<WalletEntry, Guid>().AddAsync(walletEntry);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Clear session
                await ClearBookingSessionAsync(sessionId);

                // Return finish view model with deposit information
                var finishModel = new BookingFinishDto
                {
                    Progress = new BookingProgressDto { CurrentStep = 3 },
                    BookingNumber = bookingNumber,
                    BookingStatus = "Confirmed",
                    BookingSummary = new BookingSummaryDto
                    {
                        NumberOfDays = bookingData.Summary.NumberOfDays,
                        TotalAmount = bookingData.Summary.TotalAmount,
                        TotalDeposit = bookingData.Summary.TotalDeposit,
                        PickupDate = bookingData.Summary.PickupDate,
                        ReturnDate = bookingData.Summary.ReturnDate,
                        NumberOfCars = bookingData.Summary.NumberOfCars,
                        CarItems = bookingData.Summary.CarItems
                    },
                    PaymentMethod = GetPaymentMethodDisplayName(paymentMethod),
                    PaymentStatus = paymentMethod == PaymentMethodTypeEnum.Wallet ? "Paid" : "Pending"
                };

                SetNextSteps(finishModel, paymentMethod);

                _logger.LogInformation("Successfully processed booking {BookingNumber} for user {UserId}", bookingNumber, userId);
                return finishModel;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error processing booking for session {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves booking finish information for a completed booking by booking number.
        /// Used for displaying confirmation details after successful booking creation.
        /// </summary>
        /// <param name="bookingNumber">The unique booking number to retrieve.</param>
        /// <returns>A booking finish DTO with complete confirmation information.</returns>
        public async Task<BookingFinishDto> GetBookingFinishAsync(string bookingNumber)
        {
            var booking = await _unitOfWork.Bookings.GetByBookingNumberAsync(bookingNumber);

            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            // Calculate total deposit from booking items
            var totalDeposit = booking.BookingItems.Sum(bi => bi.Deposit ?? 0);
            var nuberOfDay = await CalculateNumberOfDaysAsync(booking.PickupDate ?? DateTime.Today, booking.ReturnDate ?? DateTime.Today.AddDays(1));

            var model = new BookingFinishDto
            {
                Progress = new BookingProgressDto { CurrentStep = 3 },
                BookingNumber = bookingNumber,
                BookingStatus = "Confirmed",
                BookingDate = booking.CreatedAt,
                PaymentMethod = GetPaymentMethodDisplayName(Enum.Parse<PaymentMethodTypeEnum>(booking.TransactionType ?? "Cash")),
                PaymentStatus = booking.TransactionType == "Wallet" ? "Paid" : "Pending",
                BookingSummary = new BookingSummaryDto
                {
                    TotalAmount = booking.TotalAmount ?? 0,
                    TotalDeposit = totalDeposit,
                    NumberOfDays = nuberOfDay,
                    PickupDate = booking.PickupDate ?? DateTime.Today,
                    ReturnDate = booking.ReturnDate ?? DateTime.Today.AddDays(1),
                    NumberOfCars = booking.BookingItems.Count,
                    CarItems = booking.BookingItems.Select(bi => new CarSummaryItemDto
                    {
                        CarId = bi.CarID,
                        CarName = $"{bi.Car?.Brand} {bi.Car?.Model}",
                        PricePerDay = bi.PricePerDay ?? 0,
                        Deposit = bi.Deposit ?? 0,
                        SubTotal = (bi.PricePerDay ?? 0) * nuberOfDay
                    }).ToList()
                }
            };

            SetNextSteps(model, Enum.Parse<PaymentMethodTypeEnum>(booking.TransactionType ?? "Cash"));
            return model;
        }

        #endregion

        #region New Unified Booking Process

        /// <summary>
        /// Processes a complete booking using the unified booking process flow.
        /// Handles validation, persistence, and payment processing in a single transaction.
        /// </summary>
        /// <param name="bookingData">The complete booking data to process.</param>
        /// <param name="paymentMethod">The selected payment method for the booking.</param>
        /// <param name="userId">The user ID creating the booking.</param>
        /// <returns>A booking process result with confirmation details or error information.</returns>
        public async Task<BookingProcessResult> ProcessBookingAsync(BookingProcessDto bookingData, PaymentMethodTypeEnum paymentMethod, Guid userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate booking data
                var validationResult = await ValidateBookingProcessAsync(bookingData);
                if (!validationResult.IsValid)
                {
                    return BookingProcessResult.ValidationFailure(
                        validationResult.Errors.Select(e => $"{e.Field}: {e.Message}").ToList()
                    );
                }

                // Generate booking number
                var bookingNumber = await GenerateBookingNumberAsync();

                // Create main booking entity
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    BookingNo = bookingNumber,
                    RenterID = userId,
                    PickupDate = bookingData.PickupDate,
                    ReturnDate = bookingData.ReturnDate,
                    TotalAmount = bookingData.TotalAmount,
                    TransactionType = paymentMethod.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Bookings.AddAsync(booking);

                // Create booking items for each car
                for (int i = 0; i < bookingData.CarItems.Count && i < bookingData.Drivers.Count; i++)
                {
                    var carItem = bookingData.CarItems[i];
                    var driver = bookingData.Drivers[i];
                    
                    var car = await _unitOfWork.Cars.GetByIdAsync(carItem.CarId);
                    if (car == null) continue;

                    var bookingItem = new BookingItem
                    {
                        Id = Guid.NewGuid(),
                        BookingID = booking.Id,
                        CarID = carItem.CarId,
                        PricePerDay = car.BasePricePerDay,
                        Deposit = car.RequiredDeposit,
                        LicenseID = driver.IsSameAsRenter ? bookingData.Renter.LicenseNumber : driver.LicenseNumber,
                        LicenseImage = driver.IsSameAsRenter ? bookingData.Renter.LicenseImageUrl : driver.LicenseImageUrl,
                        Status = DetermineInitialBookingItemStatus(paymentMethod),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.BookingItems.AddAsync(bookingItem);
                }

                // Process payment based on method
                var paymentResult = await ProcessPaymentAsync(paymentMethod, userId, bookingData.TotalDeposit, booking.Id);
                if (!paymentResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BookingProcessResult.Failure(paymentResult.ErrorMessage);
                }

                // Save all changes
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully processed unified booking {BookingNumber} for user {UserId}", bookingNumber, userId);
                return BookingProcessResult.Success(bookingNumber);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error processing unified booking for user {UserId}", userId);
                return BookingProcessResult.Failure("An error occurred while processing the booking. Please try again.");
            }
        }

        /// <summary>
        /// Validates booking process data including dates, availability, and driver information.
        /// Performs comprehensive validation for the unified booking process flow.
        /// </summary>
        /// <param name="bookingData">The booking data to validate.</param>
        /// <returns>A validation result with any errors found during validation.</returns>
        private async Task<ValidationResult> ValidateBookingProcessAsync(BookingProcessDto bookingData)
        {
            var result = new ValidationResult();

            // Validate dates
            if (bookingData.PickupDate <= DateTime.Today)
            {
                result.AddError("PickupDate", "Pickup date must be after today.");
            }

            if (bookingData.ReturnDate <= bookingData.PickupDate)
            {
                result.AddError("ReturnDate", "Return date must be after pickup date.");
            }

            // Validate renter information
            if (string.IsNullOrWhiteSpace(bookingData.Renter.FullName))
            {
                result.AddError("Renter.FullName", "Renter full name is required.");
            }

            if (string.IsNullOrWhiteSpace(bookingData.Renter.Email))
            {
                result.AddError("Renter.Email", "Renter email is required.");
            }

            if (string.IsNullOrWhiteSpace(bookingData.Renter.LicenseNumber))
            {
                result.AddError("Renter.LicenseNumber", "Renter license number is required.");
            }

            // Validate drivers
            for (int i = 0; i < bookingData.Drivers.Count; i++)
            {
                var driver = bookingData.Drivers[i];
                if (!driver.IsSameAsRenter)
                {
                    if (string.IsNullOrWhiteSpace(driver.FullName))
                    {
                        result.AddError($"Drivers[{i}].FullName", $"Driver full name is required for car {i + 1}.");
                    }

                    if (string.IsNullOrWhiteSpace(driver.LicenseNumber))
                    {
                        result.AddError($"Drivers[{i}].LicenseNumber", $"Driver license number is required for car {i + 1}.");
                    }
                }
            }

            // Check car availability
            var unavailableCars = await GetUnavailableCarsAsync(bookingData.SelectedCarIds, bookingData.PickupDate, bookingData.ReturnDate);
            if (unavailableCars.Any())
            {
                foreach (var car in unavailableCars)
                {
                    result.AddError("CarAvailability", $"Car {car.Brand} {car.Model} (License plate: {car.LicensePlate}) is already booked during this period.");
                }
            }

            // Check license overlapping
            var overlappingLicenses = await GetOverlappingLicensesAsync(bookingData.Drivers, bookingData.PickupDate, bookingData.ReturnDate);
            if (overlappingLicenses.Any())
            {
                foreach (var driver in overlappingLicenses)
                {
                    result.AddError("LicenseOverlapping", $"License number {driver.LicenseNumber} is already being used for another booking during this period.");
                }
            }

            return result;
        }

        #endregion

        #region Business Logic

        /// <summary>
        /// Calculates the total rental amount for selected cars over the specified date range.
        /// Multiplies base price per day by the number of days for each car and sums the total.
        /// </summary>
        /// <param name="carIds">The list of car IDs to calculate total amount for.</param>
        /// <param name="pickupDate">The rental start date for calculation.</param>
        /// <param name="returnDate">The rental end date for calculation.</param>
        /// <returns>The total rental amount excluding deposits.</returns>
        public async Task<decimal> CalculateTotalAmountAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate)
        {
            var numberOfDays = await CalculateNumberOfDaysAsync(pickupDate, returnDate);
            var totalPricePerDay = await _unitOfWork.Cars.GetTotalPricePerDayAsync(carIds);

            return totalPricePerDay * numberOfDays;
        }

        /// <summary>
        /// Calculates the total deposit required for the selected cars.
        /// Sums all individual car deposit requirements for the booking.
        /// </summary>
        /// <param name="carIds">The list of car IDs to calculate total deposit for.</param>
        /// <returns>The total deposit amount required for all cars.</returns>
        public async Task<decimal> CalculateTotalDepositAsync(List<Guid> carIds)
        {
            return await _unitOfWork.Cars.GetTotalDepositAsync(carIds);
        }

        /// <summary>
        /// Calculates the number of rental days including partial days.
        /// Converts the time difference to a decimal day value for precise pricing.
        /// </summary>
        /// <param name="pickupDate">The rental start date.</param>
        /// <param name="returnDate">The rental end date.</param>
        /// <returns>The number of days as a decimal value.</returns>
        public Task<decimal> CalculateNumberOfDaysAsync(DateTime pickupDate, DateTime returnDate)
        {
            return Task.FromResult((decimal)((returnDate - pickupDate).TotalHours)/24);
        }

        /// <summary>
        /// Generates a unique booking number for tracking and reference purposes.
        /// Creates a sequential or formatted identifier for the new booking.
        /// </summary>
        /// <returns>A unique booking number string.</returns>
        public async Task<string> GenerateBookingNumberAsync()
        {
            return await _unitOfWork.Bookings.GenerateBookingNumberAsync();
        }

        /// <summary>
        /// Checks if all specified cars are available for the given date range.
        /// Validates against existing bookings to prevent double-booking conflicts.
        /// </summary>
        /// <param name="carIds">The list of car IDs to check availability for.</param>
        /// <param name="pickupDate">The intended pickup date.</param>
        /// <param name="returnDate">The intended return date.</param>
        /// <returns>True if all cars are available, false if any conflicts exist.</returns>
        public async Task<bool> AreCarsAvailableAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate)
        {
            return !await _unitOfWork.Bookings.HasOverlappingBookingsAsync(carIds, pickupDate, returnDate);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves detailed car information including pricing, specifications, and location data.
        /// Fetches comprehensive car details needed for booking display and calculations.
        /// </summary>
        /// <param name="carIds">The list of car IDs to retrieve information for.</param>
        /// <returns>A list of car information DTOs with complete details.</returns>
        public async Task<List<CarInformationDto>> GetCarInformationAsync(List<Guid> carIds)
        {
            var cars = await _unitOfWork.Cars.GetCarsByIdsAsync(carIds);

            return cars.Select(c => new CarInformationDto
            {
                Id = c.Id,      
                Brand = c.Brand ?? "",
                Model = c.Model ?? "",
                LicensePlate = c.LicensePlate,
                PricePerDay = c.BasePricePerDay ?? 0,
                RequiredDeposit = c.RequiredDeposit ?? 0,
                Color = c.Color ?? "",
                Seats = c.Seats,
                Transmission = c.Transmission,
                FuelType = c.FuelType,
                Status = c.Status,
                ImageUrl = c.CarDocuments.FirstOrDefault()?.FilePath ?? "/images/car_default_image.jpg",
                AverageRating = (c.Feedbacks?.Average(x => x.Stars) ?? 0) * 5f,                         
                TotalTrips = c.BookingItems.Count, 
                Location = $"{c.Address} - {c.Ward} - {c.District} - {c.Province}" ?? "Address had not been list yet"
            }).ToList();
        }

        /// <summary>
        /// Retrieves user wallet information including balance and availability status.
        /// Checks wallet existence and current balance for payment processing.
        /// </summary>
        /// <param name="userId">The user ID to retrieve wallet information for.</param>
        /// <returns>Wallet information DTO with balance and status details.</returns>
        public async Task<WalletInformationDto> GetUserWalletAsync(Guid userId)
        {
            var balance = await _unitOfWork.Wallets.GetBalanceAsync(userId);
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            
            return new WalletInformationDto
            {
                Balance = balance,
                HasWallet = wallet != null
            };
        }

        /// <summary>
        /// Retrieves user profile information for pre-populating booking forms.
        /// Fetches personal details, license information, and address data from user account.
        /// </summary>
        /// <param name="userId">The user ID to retrieve profile information for.</param>
        /// <returns>Renter information DTO with user profile details.</returns>
        public async Task<RenterInformationDto> GetUserProfileAsync(Guid userId)
        {
            // Use UserManager to get user information
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
                return new RenterInformationDto();

            return new RenterInformationDto
            {
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                DateOfBirth = user.DOB ?? DateTime.MinValue,
                LicenseNumber = user.LicenseId ?? "",
                LicenseImageUrl = user.LicenseImage ?? "",
                Address = user.Address ?? "",
                City = user.Province ?? "",
                District = user.District ?? "",
                Ward = user.Ward ?? ""
            };
        }

        /// <summary>
        /// Identifies cars that are not available for the specified date range.
        /// Returns a list of cars that have conflicting bookings during the requested period.
        /// </summary>
        /// <param name="carIds">The list of car IDs to check for availability.</param>
        /// <param name="pickupDate">The intended pickup date.</param>
        /// <param name="returnDate">The intended return date.</param>
        /// <returns>A list of car information DTOs for unavailable cars.</returns>
        public async Task<List<CarInformationDto>> GetUnavailableCarsAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate)
        {
            var allCars = await GetCarInformationAsync(carIds);
            var unavailableCars = new List<CarInformationDto>();

            foreach (var carId in carIds)
            {
                var isAvailable = await AreCarsAvailableAsync([carId], pickupDate, returnDate);
                if (!isAvailable)
                {
                    var car = allCars.FirstOrDefault(c => c.Id == carId);
                    if (car != null)
                    {
                        unavailableCars.Add(car);
                    }
                }
            }

            return unavailableCars;
        }

        /// <summary>
        /// Identifies drivers whose license numbers conflict with existing bookings.
        /// Checks for license number overlaps during the specified rental period.
        /// </summary>
        /// <param name="drivers">The list of driver information to check for conflicts.</param>
        /// <param name="pickupDate">The intended pickup date.</param>
        /// <param name="returnDate">The intended return date.</param>
        /// <returns>A list of driver information DTOs with license conflicts.</returns>
        public async Task<List<DriverInformationDto>> GetOverlappingLicensesAsync(List<DriverInformationDto> drivers, DateTime pickupDate, DateTime returnDate)
        {
            var overlappingDrivers = new List<DriverInformationDto>();

            foreach (var driver in drivers)
            {
                if (!string.IsNullOrWhiteSpace(driver.LicenseNumber))
                {
                    var hasOverlapping = await _unitOfWork.BookingItems.HasLicenseOverlappingAsync(
                        new List<string> { driver.LicenseNumber }, pickupDate, returnDate);
                    
                    if (hasOverlapping)
                    {
                        overlappingDrivers.Add(driver);
                    }
                }
            }

            return overlappingDrivers;
        }

        /// <summary>
        /// Checks if any driver license numbers have overlapping bookings during the specified period.
        /// Validates license availability to prevent multiple simultaneous rentals with the same license.
        /// </summary>
        /// <param name="drivers">The list of driver information to validate.</param>
        /// <param name="pickupDate">The intended pickup date for the booking.</param>
        /// <param name="returnDate">The intended return date for the booking.</param>
        /// <returns>True if any license conflicts exist, false if all licenses are available.</returns>
        public async Task<bool> CheckLicenseOverlappingAsync(List<DriverInformationDto> drivers, DateTime pickupDate, DateTime returnDate)
        {
            var licenseNumbers = drivers.Select(d => d.LicenseNumber).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            
            return await _unitOfWork.BookingItems.HasLicenseOverlappingAsync(licenseNumbers, pickupDate, returnDate);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts payment method enum to user-friendly display name.
        /// Provides localized or formatted names for payment method presentation.
        /// </summary>
        /// <param name="paymentMethod">The payment method enum to convert.</param>
        /// <returns>A human-readable payment method name.</returns>
        private string GetPaymentMethodDisplayName(PaymentMethodTypeEnum paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodTypeEnum.Wallet => "Digital Wallet",
                PaymentMethodTypeEnum.Cash => "Cash Payment",
                PaymentMethodTypeEnum.BankTransfer => "Bank Transfer",
                _ => "Unknown Payment Method"
            };
        }

        /// <summary>
        /// Sets appropriate next steps for the user based on the selected payment method.
        /// Provides guidance on what actions the user needs to take after booking completion.
        /// </summary>
        /// <param name="model">The booking finish model to update with next steps.</param>
        /// <param name="paymentMethod">The payment method used for the booking.</param>
        private void SetNextSteps(BookingFinishDto model, PaymentMethodTypeEnum paymentMethod)
        {
            // Define next steps based on payment method
            if (paymentMethod == PaymentMethodTypeEnum.Wallet)
            {
                model.NextSteps = new List<string>
                {
                    "Check your wallet for the transaction.",
                    "Contact support if you don't see the transaction.",
                    "Enjoy your rental car!",
                };
            }
            else
            {
                model.NextSteps = new List<string>
                {
                    "Payment is pending, please complete the payment to confirm booking.",
                    "Contact support for any payment-related issues.",
                    "Enjoy your rental car!",
                };
            }
        }

        /// <summary>
        /// Processes payment based on the selected payment method type.
        /// Routes payment processing to appropriate handler based on method selection.
        /// </summary>
        /// <param name="paymentMethod">The payment method to process.</param>
        /// <param name="userId">The user ID making the payment.</param>
        /// <param name="amount">The amount to be processed.</param>
        /// <param name="bookingId">The booking ID for transaction tracking.</param>
        /// <returns>A payment process result indicating success or failure.</returns>
        private async Task<PaymentProcessResult> ProcessPaymentAsync(PaymentMethodTypeEnum paymentMethod, Guid userId, decimal amount, Guid bookingId)
        {
            switch (paymentMethod)
            {
                case PaymentMethodTypeEnum.Wallet:
                    return await ProcessWalletPaymentAsync(userId, amount, bookingId);

                case PaymentMethodTypeEnum.Cash:
                case PaymentMethodTypeEnum.BankTransfer:
                    return PaymentProcessResult.Success();

                default:
                    return PaymentProcessResult.Failure("Unsupported payment method");
            }
        }

        /// <summary>
        /// Processes wallet payment by deducting amount from user wallet and creating transaction record.
        /// Handles wallet balance validation and creates appropriate wallet entry for audit trail.
        /// </summary>
        /// <param name="userId">The user ID whose wallet will be charged.</param>
        /// <param name="amount">The amount to deduct from the wallet.</param>
        /// <param name="bookingId">The booking ID for transaction reference.</param>
        /// <returns>A payment process result indicating success or failure with error details.</returns>
        private async Task<PaymentProcessResult> ProcessWalletPaymentAsync(Guid userId, decimal amount, Guid bookingId)
        {
            var success = await _unitOfWork.Wallets.DeductBalanceAsync(userId, amount);
            if (!success)
            {
                return PaymentProcessResult.Failure("Insufficient wallet balance or wallet not found");
            }

            // Create wallet entry for the transaction
            var userWallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (userWallet != null)
            {
                var walletEntry = new WalletEntry
                {
                    Id = Guid.NewGuid(),
                    WalletId = userWallet.Id,
                    Amount = -amount,
                    BookingId = bookingId,
                    Type = WalletEntryType.PayDeposit,
                    CreatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.Repository<WalletEntry, Guid>().AddAsync(walletEntry);
            }

            return PaymentProcessResult.Success();
        }

        /// <summary>
        /// Determines the initial booking item status based on the payment method selected.
        /// Sets appropriate status for booking workflow management based on payment completion.
        /// </summary>
        /// <param name="paymentMethod">The payment method used for the booking.</param>
        /// <returns>The appropriate initial booking item status enum.</returns>
        private BookingItemStatusEnum DetermineInitialBookingItemStatus(PaymentMethodTypeEnum paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodTypeEnum.Wallet => BookingItemStatusEnum.Confirm,
                PaymentMethodTypeEnum.Cash => BookingItemStatusEnum.PendingDeposit,
                PaymentMethodTypeEnum.BankTransfer => BookingItemStatusEnum.PendingDeposit,
                _ => BookingItemStatusEnum.PendingDeposit
            };
        }
        #endregion

        #region Payment Processing Helper Classes

        /// <summary>
        /// Represents the result of a payment processing operation.
        /// Provides success status and error messaging for payment transactions.
        /// </summary>
        private class PaymentProcessResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether the payment processing was successful.
            /// </summary>
            public bool IsSuccess { get; set; }

            /// <summary>
            /// Gets or sets the error message if payment processing failed.
            /// </summary>
            public string ErrorMessage { get; set; } = string.Empty;

            /// <summary>
            /// Creates a successful payment process result.
            /// </summary>
            /// <returns>A payment process result indicating success.</returns>
            public static PaymentProcessResult Success() => new() { IsSuccess = true };

            /// <summary>
            /// Creates a failed payment process result with error message.
            /// </summary>
            /// <param name="errorMessage">The error message describing the failure.</param>
            /// <returns>A payment process result indicating failure with error details.</returns>
            public static PaymentProcessResult Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
        }

        #endregion
    }
}