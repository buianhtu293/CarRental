using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;
using CarRental.Domain.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Booking entity.
    /// Provides specific data access operations for booking management including search, pagination and validation.
    /// </summary>
    public class BookingRepository : GenericRepository<Booking, Guid>, IBookingRepository
    {
        /// <summary>
        /// Initializes BookingRepository with database context
        /// </summary>
        /// <param name="context">Database context for data access</param>
        public BookingRepository(CarRentalDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Finds booking by booking number with complete related information
        /// </summary>
        /// <param name="bookingNumber">Booking number to search</param>
        /// <returns>Corresponding booking with booking items, car and renter information</returns>
        public async Task<Booking?> GetByBookingNumberAsync(string bookingNumber)
        {
            return await _dbSet
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Car)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.BookingNo == bookingNumber && !b.IsDeleted);
        }

        /// <summary>
        /// Retrieves booking list of a user with car and booking items information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Booking list sorted by creation date descending</returns>
        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Car)
                .Where(b => b.RenterID == userId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves booking list within specified date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Booking list with rental time within specified range</returns>
        public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Car)
                .Where(b => b.PickupDate >= startDate && b.ReturnDate <= endDate && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves booking containing specific booking item with complete car and image information
        /// </summary>
        /// <param name="bookingItemId">Booking item ID</param>
        /// <returns>Booking containing that booking item or null if not found</returns>
        public async Task<Booking?> GetBookingByBookingItemIdAsync(Guid bookingItemId)
        {
            var allowedTypes = new[] { "Front", "Back", "Left", "Right" };
            return await _dbSet
                .Include(x => x.BookingItems)
                    .ThenInclude(x => x.Car)
                        .ThenInclude(x => x.CarDocuments.Where(cd => allowedTypes.Contains(cd.DocumentType)))
                .Include(x => x.Renter)
                .Where(x => x.BookingItems.Any(bi => bi.Id == bookingItemId))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Checks if any booking overlaps with specified cars and time
        /// </summary>
        /// <param name="carIds">List of car IDs to check</param>
        /// <param name="pickupDate">Car pickup date</param>
        /// <param name="returnDate">Car return date</param>
        /// <param name="excludeBookingId">Booking ID to exclude from check</param>
        /// <returns>True if there are time overlapping bookings with cars</returns>
        public async Task<bool> HasOverlappingBookingsAsync(List<Guid> carIds, DateTime pickupDate, DateTime returnDate, Guid? excludeBookingId = null)
        {
            var query = _dbSet
                .Include(b => b.BookingItems)
                .Where(b => !b.IsDeleted &&
                           b.BookingItems.Any(bi => carIds.Contains(bi.CarID) && !bi.IsDeleted) &&
                           b.PickupDate < returnDate &&
                           b.ReturnDate > pickupDate);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Generates unique booking number in format: YYYYMMDD-XXXX
        /// </summary>
        /// <returns>New generated booking number</returns>
        public async Task<string> GenerateBookingNumberAsync()
        {
            var today = DateTime.Today;
            var datePrefix = today.ToString("yyyyMMdd");
            
            var lastBooking = await _dbSet
                .Where(b => b.BookingNo!.StartsWith(datePrefix))
                .OrderByDescending(b => b.BookingNo)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastBooking != null && lastBooking.BookingNo != null)
            {
                var lastSequence = lastBooking.BookingNo.Substring(9); // After "yyyyMMdd-"
                if (int.TryParse(lastSequence, out int lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            return $"{datePrefix}-{sequence:D4}";
        }

        /// <summary>
        /// Retrieves list of active bookings (currently in rental period)
        /// </summary>
        /// <returns>Booking list with rental time including today</returns>
        public async Task<IEnumerable<Booking>> GetActiveBookingsAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Car)
                .Include(b => b.Renter)
                .Where(b => !b.IsDeleted && 
                           b.PickupDate <= today && 
                           b.ReturnDate >= today)
                .OrderBy(b => b.PickupDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves paginated booking list with filtering and sorting options
        /// </summary>
        /// <param name="page">Page number (starting from 1)</param>
        /// <param name="pagaSize">Number of items per page</param>
        /// <param name="sortedBy">Sorting criteria (newest, oldest, highest, lowest)</param>
        /// <param name="status">Filter by booking item status</param>
        /// <param name="searchTerm">Search keyword in booking number or car name</param>
        /// <param name="userId">User ID to filter bookings</param>
        /// <returns>Paginated result containing booking list and page information</returns>
        public async Task<PagedResult<Booking>> GetBookingsPagedAsync(int page, int pagaSize, string sortedBy = "newest to latest", BookingItemStatusEnum? status = null, string? searchTerm = null, Guid? userId = null)
        {
            var allowedTypes = new[] { "Front", "Back", "Left", "Right" };

            var query = _dbSet
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Car)
                        .ThenInclude(c => c.CarDocuments.Where(cd => allowedTypes.Contains(cd.DocumentType)))
                .Include(b => b.Renter)
                .Where(b => !b.IsDeleted && b.RenterID == userId);

            if(!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.BookingNo!.Contains(searchTerm) || 
                                    x.BookingItems.Any(bi => bi.Car!.Brand!.Contains(searchTerm)));
            }

            if(status.HasValue)
            {
                query = query.Where(b => b.BookingItems.Any(x => x.Status == (BookingItemStatusEnum)status.Value));
            }

            switch(sortedBy)
            {
                case "newest":
                    query = query.OrderByDescending(b => b.CreatedAt);
                    break;
                case "oldest":
                    query = query.OrderBy(b => b.CreatedAt);
                    break;
                case "highest":
                    query = query.OrderByDescending(b => b.TotalAmount);
                    break;
                case "lowest":
                    query = query.OrderBy(b => b.TotalAmount);
                    break;
            }
            
            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((page - 1) * pagaSize)
                .Take(pagaSize)
                .ToListAsync();

            return new PagedResult<Booking>(bookings, totalCount, page, pagaSize);
        }
    }
}