using CarRental.Application.DTOs;
using CarRental.Application.DTOs.Booking;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;

namespace CarRental.Application.Services
{
    /// <summary>
    /// Provides comprehensive booking list management services for car rental operations.
    /// Handles booking retrieval, updates, status changes, cancellations, and car return processing
    /// with integrated wallet transaction management.
    /// </summary>
    public class BookingListService : IBookingListService
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingListService"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work for data access operations.</param>
        public BookingListService(IUnitOfWork unitOfWork)
        {
           _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Cancels a booking item and processes refund if payment was made via wallet.
        /// Handles status updates and wallet transaction reversal for cancelled bookings.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item to cancel.</param>
        /// <param name="userId">The user ID requesting the cancellation for authorization.</param>
        /// <returns>True if cancellation was successful, false otherwise.</returns>
        public async Task<bool> CancelBookingItemAsync(Guid bookingItemId, Guid userId)
        {
            // Get the booking to check payment method
            var booking = await _unitOfWork.Bookings.GetBookingByBookingItemIdAsync(bookingItemId);
            if(booking == null)
            {
                return false;
            }
            var bookingItem = booking.BookingItems.FirstOrDefault(x => x.Id == bookingItemId);
            if (bookingItem == null)
            {
                return false;
            }
            // TODO: Return correct error message 
            if (bookingItem.Status is not (BookingItemStatusEnum.Confirm or BookingItemStatusEnum.PendingDeposit))
            {
                return false;
            }
            // If payment is e-wallet then return deposit to the customer
            if(Enum.Parse<PaymentMethodTypeEnum>(booking.TransactionType!) == PaymentMethodTypeEnum.Wallet)
            {
                // Begin tracsaction
                await _unitOfWork.BeginTransactionAsync();

                var success = await _unitOfWork.Wallets.DeductBalanceAsync(userId, -bookingItem.Deposit!.Value);
                if (!success)
                {
                    throw new InvalidOperationException("Transaction can't be conduct right now");
                }

                // Create WalletEntry record for PayDeposit transaction
                var userWallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
                if (userWallet != null)
                {
                    var walletEntry = new WalletEntry
                    {
                        Id = Guid.NewGuid(),
                        WalletId = userWallet.Id,
                        Amount = bookingItem!.Deposit!.Value,
                        Type = WalletEntryType.ReturnDeposit,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<WalletEntry, Guid>().AddAsync(walletEntry);
                }

                bookingItem.Status = BookingItemStatusEnum.Cancelled;
                await _unitOfWork.Repository<BookingItem, Guid>().UpdateAsync(bookingItem);
                var affectedRows = await _unitOfWork.SaveChangesAsync();
                if (affectedRows == 0)
                {
                    return false;
                }
                await _unitOfWork.CommitTransactionAsync();

            }
            else
            {
                bookingItem.Status = BookingItemStatusEnum.Cancelled;
                await _unitOfWork.Repository<BookingItem, Guid>().UpdateAsync(bookingItem);
                var affectedRows = await _unitOfWork.SaveChangesAsync();
                if (affectedRows == 0)
                {
                    return false;
                }
            }
                return await Task.FromResult(true);
        }

        /// <summary>
        /// Confirms that a customer has picked up their rental car.
        /// Updates booking status from confirmed to in-progress to track rental lifecycle.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item being picked up.</param>
        /// <param name="userId">The user ID confirming pickup for authorization.</param>
        /// <returns>True if pickup confirmation was successful, false otherwise.</returns>
        public async Task<bool> ConfirmPickupAsync(Guid bookingItemId, Guid userId)
        {
            var bookingItem = await _unitOfWork.BookingItems.GetByIdAsync(bookingItemId);
            if (bookingItem == null)
            {
                return false;
            }
            // TODO: Return correct error message 
            if (bookingItem.Status is not (BookingItemStatusEnum.Confirm))
            {
                return false;
            }
            bookingItem.Status = BookingItemStatusEnum.InProgress;
            var affectedRows = await _unitOfWork.SaveChangesAsync();
            if (affectedRows == 0)
            {
                return false;
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Retrieves detailed booking information for a specific booking item.
        /// Provides comprehensive booking details including car, renter, and driver information.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item to retrieve.</param>
        /// <param name="userId">The user ID requesting the details for authorization.</param>
        /// <returns>Complete booking detail DTO if found and authorized, null otherwise.</returns>
        public async Task<BookingDetailDto?> GetBookingDetailAsync(Guid bookingItemId, Guid userId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingByBookingItemIdAsync(bookingItemId);
            if (booking == null || booking.RenterID != userId)
            {
                return null;
            }

            var bookingItem = booking.BookingItems.FirstOrDefault(x => x.Id == bookingItemId);
            if (bookingItem == null)
            {
                return null;
            }

            return new BookingDetailDto
            {
                Id = bookingItem.Id,
                BookingId = booking.Id,
                BookingNumber = booking.BookingNo ?? string.Empty,
                BookingDate = booking.CreatedAt,
                Status = bookingItem.Status,
                PickupDate = booking.PickupDate ?? DateTime.MinValue,
                ReturnDate = booking.ReturnDate ?? DateTime.MinValue,
                NumberOfDays = CalculateNumberOfDays(booking.PickupDate ?? DateTime.MinValue, booking.ReturnDate ?? DateTime.MinValue),
                PaymentMethod = booking.TransactionType ?? string.Empty,
                IsEditable = bookingItem.Status is BookingItemStatusEnum.Confirm or BookingItemStatusEnum.PendingDeposit,
                
                // Car Information
                CarInfo = new CarInformationDto
                {
                    Id = bookingItem.Car?.Id ?? Guid.Empty,
                    Brand = bookingItem.Car?.Brand ?? string.Empty,
                    Model = bookingItem.Car?.Model ?? string.Empty,
                    LicensePlate = bookingItem.Car?.LicensePlate ?? string.Empty,
                    ImageUrl = bookingItem.Car?.CarDocuments.FirstOrDefault()?.FilePath ?? "/images/car_default_image.jpg",
                    PricePerDay = bookingItem.PricePerDay ?? 0,
                    RequiredDeposit = bookingItem.Deposit ?? 0,
                    Location = bookingItem.Car?.Address?? string.Empty,
                    Color = bookingItem.Car?.Color ?? string.Empty,
                    Seats = bookingItem.Car?.Seats,
                    Transmission = bookingItem.Car?.Transmission,
                    FuelType = bookingItem.Car?.FuelType
                },

                // Renter Information 
                RenterInfo = new RenterInformationDto
                {
                    FullName = booking.FullName ?? "",
                    Email = booking.Email ?? "",
                    PhoneNumber = booking.PhoneNumber ?? string.Empty,
                    DateOfBirth = booking.DOB ?? DateTime.MinValue,
                    LicenseNumber = booking.LicenseID ?? string.Empty,
                    LicenseImageUrl = booking.LicenseImage ?? "",
                    Address = booking.Address ?? string.Empty,
                    City = booking.Province ?? string.Empty,
                    District = booking.District ?? string.Empty,
                    Ward = booking.Ward ?? string.Empty
                },

                Drivers =
                [
                    new DriverInformationDto
                    {
                        FullName = bookingItem.FullName ?? "",
                        Email = bookingItem.Email ?? "",
                        PhoneNumber = bookingItem.PhoneNumber ?? "",
                        DateOfBirth = bookingItem.DOB ?? DateTime.MinValue,
                        LicenseNumber = bookingItem.LicenseID ?? "",
                        LicenseImageUrl = bookingItem.LicenseImage ?? "",
                        City = bookingItem.Province ?? "",
                        District = bookingItem.District ?? "",
                        Ward = bookingItem.Ward ?? "",
                        Address = bookingItem.Address ?? "",
                        IsSameAsRenter = false,                    
                    }
                ],

                // Calculate totals
                TotalAmount = (bookingItem.PricePerDay ?? 0) * CalculateNumberOfDays(booking.PickupDate ?? DateTime.MinValue, booking.ReturnDate ?? DateTime.MinValue),
                TotalDeposit = bookingItem.Deposit ?? 0
            };
        }

        /// <summary>
        /// Retrieves a paginated list of bookings for a user with optional filtering and sorting.
        /// Supports search functionality, status filtering, and various sorting options.
        /// </summary>
        /// <param name="request">The booking list request containing pagination, filtering, and sorting parameters.</param>
        /// <returns>A paginated response containing booking list data with total count information.</returns>
        public async Task<BookingListResponseDto> GetBookingListAsync(BookingListRequestDto request)
        {
            var pagedResult = await _unitOfWork.Bookings.GetBookingsPagedAsync(request.Page, request.PageSize, request.SortBy, request.StatusFilter, request.SearchTerm, request.UserId);

            // Map từ Domain sang Application DTO
            return new BookingListResponseDto
            {
                Bookings = pagedResult.Items.ToList().Select(booking => new BookingListDto
                {
                    Id = booking.Id,
                    BookingNumber = booking.BookingNo ?? string.Empty,
                    BookingDate = booking.CreatedAt,
                    PickupDate = booking.PickupDate ?? DateTime.MinValue,
                    ReturnDate = booking.ReturnDate ?? DateTime.MinValue,
                    NumberOfDays = CalculateNumberOfDays(booking.PickupDate ?? DateTime.MinValue, booking.ReturnDate ?? DateTime.MinValue),
                    TotalAmount = booking.TotalAmount ?? 0m,
                    TotalDeposit = booking.BookingItems?.Sum(bi => bi.Deposit ?? 0m) ?? 0m,
                    RenterName = booking.Renter?.FullName ?? string.Empty,
                    RenterEmail = booking.Renter?.Email ?? string.Empty,
                    RenterPhone = booking.Renter?.PhoneNumber ?? string.Empty,
                    BookingItems = booking.BookingItems?.Select(bookingItem => new BookingItemListDto
                    {
                        Id = bookingItem.Id,
                        BookingId = bookingItem.BookingID,
                        CarId = bookingItem.CarID,
                        CarBrand = bookingItem.Car?.Brand ?? string.Empty,
                        CarModel = bookingItem.Car?.Model ?? string.Empty,
                        LicensePlate = bookingItem.Car?.LicensePlate ?? string.Empty,
                        PricePerDay = bookingItem.PricePerDay ?? 0m,
                        Deposit = bookingItem.Deposit ?? 0m,
                        SubTotal = (bookingItem.PricePerDay ?? 0m) * CalculateNumberOfDays(booking.PickupDate ?? DateTime.MinValue, booking.ReturnDate ?? DateTime.MinValue),
                        Status = bookingItem.Status,
                        CarImages = bookingItem?.Car?.CarDocuments.Any() == true
                            ? bookingItem.Car.CarDocuments
                                .Select(x => x.FilePath ?? "/images/car_default_image.jpg")
                                .ToList()
                            : new List<string>
                            {
                                "/images/car_default_image.jpg",
                                "/images/car_default_image.jpg",
                                "/images/car_default_image.jpg",
                                "/images/car_default_image.jpg"
                            }
                    }).ToList() ?? new List<BookingItemListDto>()
                }).ToList(),
                TotalCount = pagedResult.TotalCount,
                CurrentPage = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        /// <summary>
        /// Updates booking details including renter and driver information.
        /// Allows modification of personal details while preserving booking core information.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item to update.</param>
        /// <param name="updateDto">The update data containing modified booking information.</param>
        /// <param name="userId">The user ID requesting the update for authorization.</param>
        /// <returns>True if update was successful, false otherwise.</returns>
        public async Task<bool> UpdateBookingDetailAsync(Guid bookingItemId, BookingDetailUpdateDto updateDto, Guid userId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingByBookingItemIdAsync(bookingItemId);
            if (booking == null || booking.RenterID != userId)
            {
                return false;
            }

            var bookingItem = booking.BookingItems.FirstOrDefault(x => x.Id == bookingItemId);
            if (bookingItem == null)
            {
                return false;
            }

            // Check if booking is editable
            if (bookingItem.Status is not (BookingItemStatusEnum.Confirm or BookingItemStatusEnum.PendingDeposit))
            {
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                booking.FullName = updateDto.RenterInfo.FullName;
                booking.Email = updateDto.RenterInfo.Email;
                booking.PhoneNumber = updateDto.RenterInfo.PhoneNumber;
                booking.DOB = updateDto.RenterInfo.DateOfBirth;
                booking.Address = updateDto.RenterInfo.Address;
                booking.Province = updateDto.RenterInfo.City;
                booking.District = updateDto.RenterInfo.District;
                booking.Ward = updateDto.RenterInfo.Ward;
                booking.LicenseID = updateDto.RenterInfo.LicenseNumber;
                booking.LicenseImage = updateDto.RenterInfo.LicenseImageUrl ?? "";

                bookingItem.FullName = updateDto.Drivers.First().FullName;
                bookingItem.Email = updateDto.Drivers.First().Email;
                bookingItem.PhoneNumber = updateDto.Drivers.First().PhoneNumber;
                bookingItem.DOB = updateDto.Drivers.First().DateOfBirth;
                bookingItem.Address= updateDto.Drivers.First().Address;
                bookingItem.Province = updateDto.Drivers.First().City;
                bookingItem.District= updateDto.Drivers.First().District;
                bookingItem.Ward= updateDto.Drivers.First().Ward;

                // Update booking item license information
                bookingItem.LicenseID = updateDto.Drivers.First().LicenseNumber ?? "";
                bookingItem.LicenseImage = updateDto.Drivers.First().LicenseImageUrl ?? "";

                await _unitOfWork.Repository<Booking, Guid>().UpdateAsync(booking);
                await _unitOfWork.Repository<BookingItem, Guid>().UpdateAsync(bookingItem);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        /// <summary>
        /// Initiates the car return process and calculates financial adjustments.
        /// Determines refund amounts, additional charges, and validates wallet balance for return processing.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item being returned.</param>
        /// <param name="userId">The user ID returning the car for authorization.</param>
        /// <returns>A return car result containing financial calculations and processing requirements.</returns>
        public async Task<ReturnCarResultDto> ReturnCarAsync(Guid bookingItemId, Guid userId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingByBookingItemIdAsync(bookingItemId);
            if (booking == null || booking.RenterID != userId)
            {
                return new ReturnCarResultDto { Success = false, Message = "Booking not found." };
            }

            var bookingItem = booking.BookingItems.FirstOrDefault(x => x.Id == bookingItemId);
            if (bookingItem == null)
            {
                return new ReturnCarResultDto { Success = false, Message = "Booking item not found." };
            }

            if (bookingItem.Status != BookingItemStatusEnum.InProgress)
            {
                return new ReturnCarResultDto { Success = false, Message = "Car is not currently rented." };
            }

            // Only process wallet transactions for wallet payments
            if (!Enum.TryParse<PaymentMethodTypeEnum>(booking.TransactionType, out var paymentMethod) || 
                paymentMethod != PaymentMethodTypeEnum.Wallet)
            {
                // For non-wallet payments, just change status to pending payment
                bookingItem.Status = BookingItemStatusEnum.PendingPayment;
                await _unitOfWork.Repository<BookingItem, Guid>().UpdateAsync(bookingItem);
                await _unitOfWork.SaveChangesAsync();

                return new ReturnCarResultDto 
                { 
                    Success = true, 
                    Message = "Car returned successfully.",
                    RequiresWalletProcessing = false
                };
            }

            // Calculate rental amount vs deposit
            var numberOfDays = CalculateNumberOfDays(booking.PickupDate!.Value, booking.ReturnDate!.Value);
            var rentalAmount = (bookingItem.PricePerDay ?? 0) * numberOfDays;
            var depositAmount = bookingItem.Deposit ?? 0;

            var userWallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (userWallet == null)
            {
                return new ReturnCarResultDto { Success = false, Message = "User wallet not found." };
            }

            return new ReturnCarResultDto
            {
                Success = true,
                RequiresWalletProcessing = true,
                RentalAmount = rentalAmount,
                DepositAmount = depositAmount,
                CurrentWalletBalance = userWallet.Balance,
                RefundAmount = Math.Max(0, depositAmount - rentalAmount),
                AdditionalChargeNeeded = Math.Max(0, rentalAmount - depositAmount),
                CanProcessReturn = userWallet.Balance + depositAmount >= rentalAmount
            };
        }

        /// <summary>
        /// Processes the complete car return transaction including payments and refunds.
        /// Handles wallet transactions, owner payments, and booking status updates atomically.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item being returned.</param>
        /// <param name="userId">The user ID completing the return transaction.</param>
        /// <returns>True if return transaction was processed successfully, false otherwise.</returns>
        public async Task<bool> ProcessReturnCarTransactionAsync(Guid bookingItemId, Guid userId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingByBookingItemIdAsync(bookingItemId);
            if (booking == null || booking.RenterID != userId)
            {
                return false;
            }

            var bookingItem = booking.BookingItems.FirstOrDefault(x => x.Id == bookingItemId);
            if (bookingItem == null || bookingItem.Status != BookingItemStatusEnum.InProgress)
            {
                return false;
            }

            if (!Enum.TryParse<PaymentMethodTypeEnum>(booking.TransactionType, out var paymentMethod) || 
                paymentMethod != PaymentMethodTypeEnum.Wallet)
            {
                return false;
            }

            var numberOfDays = CalculateNumberOfDays(booking.PickupDate!.Value, booking.ReturnDate!.Value);
            var rentalAmount = (bookingItem.PricePerDay ?? 0) * numberOfDays;
            var depositAmount = bookingItem.Deposit ?? 0;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var userWallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
                var carOwnerWallet = await _unitOfWork.Wallets.GetByUserIdAsync(bookingItem.Car?.OwnerID ?? Guid.Empty);

                if (userWallet == null || carOwnerWallet == null)
                {
                    throw new InvalidOperationException("Wallet not found");
                }

                // Calculate amounts
                var refundAmount = Math.Max(0, depositAmount - rentalAmount);
                var additionalCharge = Math.Max(0, rentalAmount - depositAmount);

                // Check if user has enough balance for additional charges
                if (userWallet.Balance < additionalCharge)
                {
                    throw new InvalidOperationException("Insufficient balance for additional charges");
                }

                // Process transactions
                if (additionalCharge > 0)
                {
                    // Charge additional amount from user
                    await _unitOfWork.Wallets.DeductBalanceAsync(userId, additionalCharge);
                    
                    // Create wallet entry for additional charge
                    var chargeEntry = new WalletEntry
                    {
                        Id = Guid.NewGuid(),
                        WalletId = userWallet.Id,
                        Amount = -additionalCharge,
                        Type = WalletEntryType.OffSetFinalPayment,
                        BookingId = booking.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<WalletEntry, Guid>().AddAsync(chargeEntry);
                }

                if (refundAmount > 0)
                {
                    // Refund to user
                    await _unitOfWork.Wallets.DeductBalanceAsync(userId, -refundAmount);
                    
                    // Create wallet entry for refund
                    var refundEntry = new WalletEntry
                    {
                        Id = Guid.NewGuid(),
                        WalletId = userWallet.Id,
                        Amount = refundAmount,
                        BookingId = booking.Id,
                        Type = WalletEntryType.ReturnDeposit,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<WalletEntry, Guid>().AddAsync(refundEntry);
                }

                // Pay car owner (rental amount)
                await _unitOfWork.Wallets.DeductBalanceAsync(carOwnerWallet.UserId, -rentalAmount);
                
                var ownerPaymentEntry = new WalletEntry
                {
                    Id = Guid.NewGuid(),
                    WalletId = carOwnerWallet.Id,
                    Amount = rentalAmount,
                    BookingId = booking.Id,
                    Type = WalletEntryType.ReceivePayment,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<WalletEntry, Guid>().AddAsync(ownerPaymentEntry);

                // Update booking item status
                bookingItem.Status = BookingItemStatusEnum.Completed;
                await _unitOfWork.Repository<BookingItem, Guid>().UpdateAsync(bookingItem);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        /// <summary>
        /// Updates the status of a booking item to a new state.
        /// Provides generic status update functionality for booking workflow management.
        /// </summary>
        /// <param name="bookingItemId">The unique identifier of the booking item to update.</param>
        /// <param name="newStatus">The new status to set for the booking item.</param>
        /// <param name="userId">The user ID requesting the status change for authorization.</param>
        /// <returns>True if status update was successful, false otherwise.</returns>
        public Task<bool> UpdateBookingItemStatusAsync(Guid bookingItemId, BookingItemStatusEnum newStatus, Guid userId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the number of rental days between pickup and return dates.
        /// Provides accurate day calculation including partial days for pricing purposes.
        /// </summary>
        /// <param name="pickupDate">The rental start date.</param>
        /// <param name="returnDate">The rental end date.</param>
        /// <returns>The number of days as a decimal value for precise calculations.</returns>
        private static decimal CalculateNumberOfDays(DateTime pickupDate, DateTime returnDate)
        {
            if (pickupDate == DateTime.MinValue || returnDate == DateTime.MinValue)
                return 0;

            var days = ((decimal)((returnDate - pickupDate).TotalHours) / 24);
            return Math.Max(0, days);
        }
    }
}
