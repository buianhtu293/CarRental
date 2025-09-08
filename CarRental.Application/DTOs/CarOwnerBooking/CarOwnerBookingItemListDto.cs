using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs.CarOwnerBooking
{
    public class CarOwnerBookingItemListDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid CarId { get; set; }
        public Guid OwnerId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string CarName => $"{CarBrand} {CarModel}";
        public string LicensePlate { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal Deposit { get; set; }
        public decimal SubTotal { get; set; }
        public BookingItemStatusEnum Status { get; set; }
        public string StatusDisplay => GetStatusDisplay();
        public string StatusClass => GetStatusClass();
        public List<string> CarImages { get; set; } = new();
        public string PrimaryImage => CarImages.FirstOrDefault() ?? "/img/car-rent-1.png";

        private string GetStatusDisplay()
        {
            return Status switch
            {
                BookingItemStatusEnum.PendingDeposit => "Pending Deposit",
                BookingItemStatusEnum.Confirm => "Confirmed",
                BookingItemStatusEnum.InProgress => "In Progress",
                BookingItemStatusEnum.PendingPayment => "Pending Payment",
                BookingItemStatusEnum.Completed => "Completed",
                BookingItemStatusEnum.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        private string GetStatusClass()
        {
            return Status switch
            {
                BookingItemStatusEnum.PendingDeposit => "badge-warning",
                BookingItemStatusEnum.Confirm => "badge-success",
                BookingItemStatusEnum.InProgress => "badge-info",
                BookingItemStatusEnum.PendingPayment => "badge-warning",
                BookingItemStatusEnum.Completed => "badge-success",
                BookingItemStatusEnum.Cancelled => "badge-danger",
                _ => "badge-secondary"
            };
        }
    }
}
