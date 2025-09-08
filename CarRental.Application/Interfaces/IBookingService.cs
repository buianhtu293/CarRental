using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.Application.Interfaces
{
    /// <summary>
    /// Interface defining booking management services including session creation, information processing, payment and booking completion.
    /// Provides methods to perform the complete car rental booking process from initialization to completion.
    /// </summary>
    public interface IBookingService
    {
        #region Session Management (Legacy)
        
        /// <summary>
        /// Creates a new booking session for specified cars and date range
        /// </summary>
        /// <param name="carIds">List of car IDs to book</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>Session ID to track booking process</returns>
        Task<string> CreateBookingSessionAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate);
        
        /// <summary>
        /// Retrieves booking information from session cache
        /// </summary>
        /// <param name="sessionId">Session ID to retrieve information</param>
        /// <returns>Booking information if session exists, null otherwise</returns>
        Task<BookingInformationDto?> GetBookingSessionAsync(string sessionId);
        
        /// <summary>
        /// Updates booking information in session cache
        /// </summary>
        /// <param name="sessionId">Session ID to update</param>
        /// <param name="model">New booking information</param>
        Task UpdateBookingSessionAsync(string sessionId, BookingInformationDto model);
        
        /// <summary>
        /// Clears booking data from session cache
        /// </summary>
        /// <param name="sessionId">Session ID to clear</param>
        Task ClearBookingSessionAsync(string sessionId);

        #endregion

        #region Step 1: Booking Information
        
        /// <summary>
        /// Initializes initial booking information with selected cars and user information
        /// </summary>
        /// <param name="carIds">List of selected car IDs</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <param name="userId">User ID (optional to pre-fill information)</param>
        /// <returns>DTO containing initialized booking information</returns>
        Task<BookingInformationDto> InitializeBookingInformationAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate, Guid? userId = null);
        
        /// <summary>
        /// Validates booking information including dates, renter and driver information
        /// </summary>
        /// <param name="model">Booking information to validate</param>
        /// <returns>Validation result with error list (if any)</returns>
        Task<ValidationResult> ValidateBookingInformationAsync(BookingInformationDto model);

        #endregion

        #region Step 2: Payment
        
        /// <summary>
        /// Initializes payment information including available methods and wallet information
        /// </summary>
        /// <param name="sessionId">Session ID containing booking information</param>
        /// <param name="userId">User ID to retrieve wallet information</param>
        /// <returns>DTO containing payment information</returns>
        Task<PaymentDto> InitializePaymentAsync(string sessionId, Guid userId);
        
        /// <summary>
        /// Validates payment information and selected method
        /// </summary>
        /// <param name="model">Payment information to validate</param>
        /// <returns>Validation result</returns>
        Task<ValidationResult> ValidatePaymentAsync(PaymentDto model);

        #endregion

        #region Step 3: Finish Booking
        
        /// <summary>
        /// Retrieves completed booking information by booking number
        /// </summary>
        /// <param name="bookingNumber">Booking number to retrieve information</param>
        /// <returns>DTO containing completed booking information</returns>
        Task<BookingFinishDto> GetBookingFinishAsync(string bookingNumber);

        #endregion

        #region New Unified Booking Process
        
        /// <summary>
        /// Processes booking using new unified flow (all in one transaction)
        /// </summary>
        /// <param name="bookingData">Complete booking data</param>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="userId">User ID creating booking</param>
        /// <returns>Booking processing result</returns>
        Task<BookingProcessResult> ProcessBookingAsync(BookingProcessDto bookingData, PaymentMethodTypeEnum paymentMethod, Guid userId);

        #endregion

        #region Business Logic
        
        /// <summary>
        /// Calculates total rental amount for car list in specified time period
        /// </summary>
        /// <param name="carIds">List of car IDs</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>Total rental amount</returns>
        Task<decimal> CalculateTotalAmountAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate);
        
        /// <summary>
        /// Calculates total deposit amount for car list
        /// </summary>
        /// <param name="carIds">List of car IDs</param>
        /// <returns>Total deposit amount</returns>
        Task<decimal> CalculateTotalDepositAsync(List<Guid> carIds);
        
        /// <summary>
        /// Calculates number of rental days (can be decimal)
        /// </summary>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>Number of rental days</returns>
        Task<decimal> CalculateNumberOfDaysAsync(DateTime pickupDate, DateTime returnDate);
        
        /// <summary>
        /// Generates unique booking number
        /// </summary>
        /// <returns>Booking number</returns>
        Task<string> GenerateBookingNumberAsync();
        
        /// <summary>
        /// Checks if cars are available in specified time period
        /// </summary>
        /// <param name="carIds">List of car IDs to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>True if all cars are available</returns>
        Task<bool> AreCarsAvailableAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate);

        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Retrieves detailed information of car list
        /// </summary>
        /// <param name="carIds">List of car IDs</param>
        /// <returns>List of car information</returns>
        Task<List<CarInformationDto>> GetCarInformationAsync(List<Guid> carIds);
        
        /// <summary>
        /// Retrieves user's wallet information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Wallet information</returns>
        Task<WalletInformationDto> GetUserWalletAsync(Guid userId);
        
        /// <summary>
        /// Retrieves user profile information to pre-fill form
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Renter information</returns>
        Task<RenterInformationDto> GetUserProfileAsync(Guid userId);
        
        /// <summary>
        /// Retrieves list of unavailable cars in specified time period
        /// </summary>
        /// <param name="carIds">List of car IDs to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>List of unavailable cars</returns>
        Task<List<CarInformationDto>> GetUnavailableCarsAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate);
        
        /// <summary>
        /// Retrieves list of drivers with overlapping driver license in specified time period
        /// </summary>
        /// <param name="drivers">List of drivers to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>List of drivers with overlapping licenses</returns>
        Task<List<DriverInformationDto>> GetOverlappingLicensesAsync(List<DriverInformationDto> drivers, DateTime pickupDate, DateTime returnDate);
        
        /// <summary>
        /// Checks if any driver license overlaps in specified time period
        /// </summary>
        /// <param name="drivers">List of drivers to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>True if any license overlaps</returns>
        Task<bool> CheckLicenseOverlappingAsync(List<DriverInformationDto> drivers, DateTime pickupDate, DateTime returnDate);

        #endregion
    }
}