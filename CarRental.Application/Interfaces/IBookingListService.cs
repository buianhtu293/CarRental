using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Enums;

namespace CarRental.Application.Interfaces
{
    /// <summary>
    /// Interface defining user booking list management services.
    /// Provides functionalities to view, edit, update status and process car returns.
    /// </summary>
    public interface IBookingListService
    {
        /// <summary>
        /// Retrieves user's booking list with pagination and filtering
        /// </summary>
        /// <param name="request">Search and pagination parameters</param>
        /// <returns>Booking list with pagination information</returns>
        Task<BookingListResponseDto> GetBookingListAsync(BookingListRequestDto request);
        
        /// <summary>
        /// Retrieves detailed information of a booking item
        /// </summary>
        /// <param name="bookingItemId">Booking item ID</param>
        /// <param name="userId">User ID (to check access permission)</param>
        /// <returns>Booking item detailed information or null if not found</returns>
        Task<BookingDetailDto?> GetBookingDetailAsync(Guid bookingItemId, Guid userId);
        
        /// <summary>
        /// Updates detailed information of booking item
        /// </summary>
        /// <param name="bookingItemId">Booking item ID to update</param>
        /// <param name="updateDto">New information to update</param>
        /// <param name="userId">User ID (to check permission)</param>
        /// <returns>True if update successful</returns>
        Task<bool> UpdateBookingDetailAsync(Guid bookingItemId, BookingDetailUpdateDto updateDto, Guid userId);
        
        /// <summary>
        /// Updates booking item status
        /// </summary>
        /// <param name="bookingItemId">Booking item ID</param>
        /// <param name="newStatus">New status</param>
        /// <param name="userId">User ID (to check permission)</param>
        /// <returns>True if update successful</returns>
        Task<bool> UpdateBookingItemStatusAsync(Guid bookingItemId, BookingItemStatusEnum newStatus, Guid userId);
        
        /// <summary>
        /// Cancels booking item
        /// </summary>
        /// <param name="bookingItemId">Booking item ID to cancel</param>
        /// <param name="userId">User ID (to check permission)</param>
        /// <returns>True if cancellation successful</returns>
        Task<bool> CancelBookingItemAsync(Guid bookingItemId, Guid userId);
        
        /// <summary>
        /// Confirms car pickup
        /// </summary>
        /// <param name="bookingItemId">Booking item ID</param>
        /// <param name="userId">User ID (to check permission)</param>
        /// <returns>True if confirmation successful</returns>
        Task<bool> ConfirmPickupAsync(Guid bookingItemId, Guid userId);
        
        /// <summary>
        /// Processes car return and calculates financial information
        /// </summary>
        /// <param name="bookingItemId">Booking item ID to return car</param>
        /// <param name="userId">User ID (to check permission)</param>
        /// <returns>Car return processing result with financial information</returns>
        Task<ReturnCarResultDto> ReturnCarAsync(Guid bookingItemId, Guid userId);
        
        /// <summary>
        /// Processes financial transaction when returning car (refund or additional charge)
        /// </summary>
        /// <param name="bookingItemId">Booking item ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if transaction processing successful</returns>
        Task<bool> ProcessReturnCarTransactionAsync(Guid bookingItemId, Guid userId);
    }
}
