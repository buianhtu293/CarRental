using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs.CarOwnerBooking
{
    public class CarOwnerBookingListRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "newest"; // newest, oldest
        public BookingItemStatusEnum? StatusFilter { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }
}
