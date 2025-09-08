using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;
using CarRental.Domain.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    public class CarOwnerBookingRepository : GenericRepository<Booking, Guid>, ICarOwnerBookingRepository
    {
        public CarOwnerBookingRepository(CarRentalDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Booking>> GetBookingsPagedAsync(int page, int pagaSize, string sortedBy = "newest to latest", BookingItemStatusEnum? status = null, string? searchTerm = null, Guid? userId = null)
        {
            // Co nen chuyen thanh iQueryable o day khong 
            var query = _dbSet
    .Include(b => b.BookingItems)
        .ThenInclude(bi => bi.Car)
        .ThenInclude(doc => doc.CarDocuments)
    .Where(b => !b.IsDeleted
        && b.BookingItems.Any(bi => bi.Car.OwnerID == userId));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.BookingNo!.Contains(searchTerm) ||
                                    x.BookingItems.Any(bi => bi.Car!.Brand!.Contains(searchTerm)));
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.BookingItems.Any(x => x.Status == (BookingItemStatusEnum)status.Value));
            }

            switch (sortedBy)
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
