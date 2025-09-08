using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Models;

namespace CarRental.Domain.Interfaces
{
    public interface ICarOwnerBookingRepository : IGenericRepository<Booking, Guid>
    {
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
