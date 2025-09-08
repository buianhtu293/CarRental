using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application.DTOs.Booking;

namespace CarRental.Application.DTOs.CarOwnerBooking
{
    public class CarOwnerBookingListDto
    {
        public Guid Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDeposit { get; set; }
        public string RenterName { get; set; } = string.Empty;
        public string RenterEmail { get; set; } = string.Empty;
        public string RenterPhone { get; set; } = string.Empty;
        public List<CarOwnerBookingItemListDto> BookingItems { get; set; } = new();
    }
}
