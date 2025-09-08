using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Models;

namespace CarRental.Domain.Interfaces
{
    /// <summary>
    /// Interface defining data access methods for Booking entity.
    /// Provides CRUD operations and special queries for booking management.
    /// </summary>
    public interface IBookingRepository : IGenericRepository<Booking, Guid>
    {
        /// <summary>
        /// Finds booking by booking number
        /// </summary>
        /// <param name="bookingNumber">Booking number to search</param>
        /// <returns>Corresponding booking or null if not found</returns>
        Task<Booking?> GetByBookingNumberAsync(string bookingNumber);
        
        /// <summary>
        /// Retrieves booking list of a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's booking list</returns>
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(Guid userId);
        
        /// <summary>
        /// Retrieves booking list within specified date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Booking list within date range</returns>
        Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Retrieves booking containing specific booking item
        /// </summary>
        /// <param name="bookingItemId">Booking item ID</param>
        /// <returns>Booking containing that booking item</returns>
        Task<Booking> GetBookingByBookingItemIdAsync(Guid bookingItemId);
        
        /// <summary>
        /// Checks if any booking overlaps with specified cars and time
        /// </summary>
        /// <param name="carIds">List of car IDs to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <param name="excludeBookingId">Booking ID to exclude from check (optional)</param>
        /// <returns>True if there are overlapping bookings</returns>
        Task<bool> HasOverlappingBookingsAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate, Guid? excludeBookingId = null);
        
        /// <summary>
        /// Generates new unique booking number
        /// </summary>
        /// <returns>New booking number</returns>
        Task<string> GenerateBookingNumberAsync();
        
        /// <summary>
        /// Retrieves list of active bookings (not completed)
        /// </summary>
        /// <returns>List of active bookings</returns>
        Task<IEnumerable<Booking>> GetActiveBookingsAsync();
        
        /// <summary>
        /// Retrieves paginated booking list with filtering
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortBy">Sorting criteria (newest, oldest)</param>
        /// <param name="status">Filter by status (optional)</param>
        /// <param name="searchTerm">Search keyword (optional)</param>
        /// <param name="userId">User ID to filter bookings (optional)</param>
        /// <returns>Paginated result containing booking list</returns>
        Task<PagedResult<Booking>> GetBookingsPagedAsync(
            int page,
            int pageSize,
            string sortBy = "newest",
            BookingItemStatusEnum? status = null,
            string? searchTerm = null,
            Guid? userId = null
        );
    }
}