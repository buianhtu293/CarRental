using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;

namespace CarRental.Infrastructure.Repositories
{
    /// <summary>
    /// Provides data access operations for booking items including conflict detection,
    /// license overlap checking, and relationship management with bookings and cars.
    /// </summary>
    public class BookingItemRepository : GenericRepository<BookingItem, Guid>, IBookingItemRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BookingItemRepository"/> class.
        /// </summary>
        /// <param name="context">The database context for data access operations.</param>
        public BookingItemRepository(CarRentalDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all booking items associated with a specific booking.
        /// Includes related car and booking information for comprehensive data access.
        /// </summary>
        /// <param name="bookingId">The unique identifier of the booking to retrieve items for.</param>
        /// <returns>A collection of booking items with related entities loaded.</returns>
        public async Task<IEnumerable<BookingItem>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _dbSet
                .Include(bi => bi.Car)
                .Include(bi => bi.Booking)
                .Where(bi => bi.BookingID == bookingId && !bi.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all booking items for a specific car ordered by creation date.
        /// Useful for car rental history and availability analysis.
        /// </summary>
        /// <param name="carId">The unique identifier of the car to retrieve booking history for.</param>
        /// <returns>A collection of booking items for the specified car in descending date order.</returns>
        public async Task<IEnumerable<BookingItem>> GetByCarIdAsync(Guid carId)
        {
            return await _dbSet
                .Include(bi => bi.Booking)
                .Include(bi => bi.Car)
                .Where(bi => bi.CarID == carId && !bi.IsDeleted)
                .OrderByDescending(bi => bi.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Checks for license number conflicts during a specified date range.
        /// Prevents double-booking with the same driver license and validates license uniqueness.
        /// </summary>
        /// <param name="licenseNumbers">The list of license numbers to check for conflicts.</param>
        /// <param name="pickupDate">The intended pickup date for conflict checking.</param>
        /// <param name="returnDate">The intended return date for conflict checking.</param>
        /// <returns>True if any license conflicts exist, false if all licenses are available.</returns>
        public async Task<bool> HasLicenseOverlappingAsync(List<string> licenseNumbers, DateTime pickupDate, DateTime returnDate)
        {
            if (!licenseNumbers.Any()) return false;

            // Check for duplicate licenses in the same booking
            if (licenseNumbers.Count != licenseNumbers.Distinct().Count())
            {
                return true;
            }

            // Check against existing bookings
            return await _dbSet
                .Include(bi => bi.Booking)
                .Where(bi => licenseNumbers.Contains(bi.LicenseID) &&
                           bi.Booking.PickupDate < returnDate &&
                           bi.Booking.ReturnDate > pickupDate &&
                           !bi.IsDeleted &&
                           !bi.Booking.IsDeleted)
                .AnyAsync();
        }

        /// <summary>
        /// Retrieves booking items that have date conflicts with the specified cars and date range.
        /// Identifies existing bookings that would prevent new bookings for the same cars.
        /// </summary>
        /// <param name="carIds">The list of car IDs to check for booking conflicts.</param>
        /// <param name="pickupDate">The intended pickup date for conflict detection.</param>
        /// <param name="returnDate">The intended return date for conflict detection.</param>
        /// <returns>A collection of conflicting booking items with related data loaded.</returns>
        public async Task<IEnumerable<BookingItem>> GetOverlappingBookingItemsAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate)
        {
            return await _dbSet
                .Include(bi => bi.Booking)
                .Include(bi => bi.Car)
                .Where(bi => carIds.Contains(bi.CarID) &&
                           bi.Booking.PickupDate < returnDate &&
                           bi.Booking.ReturnDate > pickupDate &&
                           !bi.IsDeleted &&
                           !bi.Booking.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all booking items associated with a specific driver license number.
        /// Useful for driver history analysis and license verification processes.
        /// </summary>
        /// <param name="licenseNumber">The driver license number to search for.</param>
        /// <returns>A collection of booking items for the specified license in descending date order.</returns>
        public async Task<IEnumerable<BookingItem>> GetByLicenseNumberAsync(string licenseNumber)
        {
            return await _dbSet
                .Include(bi => bi.Booking)
                .Include(bi => bi.Car)
                .Where(bi => bi.LicenseID == licenseNumber && !bi.IsDeleted)
                .OrderByDescending(bi => bi.CreatedAt)
                .ToListAsync();
        }
    }
}