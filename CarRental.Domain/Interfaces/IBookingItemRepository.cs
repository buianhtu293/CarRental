using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces
{
    /// <summary>
    /// Interface defining data access methods for BookingItem entity.
    /// Provides CRUD operations and special queries for booking item management.
    /// </summary>
    public interface IBookingItemRepository : IGenericRepository<BookingItem, Guid>
    {
        /// <summary>
        /// Retrieves booking item list belonging to a specific booking
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>List of booking items of that booking</returns>
        Task<IEnumerable<BookingItem>> GetByBookingIdAsync(Guid bookingId);
        
        /// <summary>
        /// Retrieves booking item list of a specific car
        /// </summary>
        /// <param name="carId">Car ID</param>
        /// <returns>List of booking items of that car</returns>
        Task<IEnumerable<BookingItem>> GetByCarIdAsync(Guid carId);
        
        /// <summary>
        /// Checks if any booking item uses overlapping driver license in specified time period
        /// </summary>
        /// <param name="licenseNumbers">List of driver license numbers to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>True if there are overlapping licenses</returns>
        Task<bool> HasLicenseOverlappingAsync(List<string> licenseNumbers, DateTime pickupDate, DateTime returnDate);
        
        /// <summary>
        /// Retrieves booking item list overlapping with specified cars and time
        /// </summary>
        /// <param name="carIds">List of car IDs to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <returns>List of overlapping booking items</returns>
        Task<IEnumerable<BookingItem>> GetOverlappingBookingItemsAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate);
        
        /// <summary>
        /// Retrieves booking item list by driver license number
        /// </summary>
        /// <param name="licenseNumber">Driver license number</param>
        /// <returns>List of booking items using that license</returns>
        Task<IEnumerable<BookingItem>> GetByLicenseNumberAsync(string licenseNumber);
    }
}