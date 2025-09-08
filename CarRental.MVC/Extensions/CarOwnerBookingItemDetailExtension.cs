using CarRental.Application.DTOs.Booking;
using CarRental.MVC.Models.CarOwnerBooking;

namespace CarRental.MVC.Extensions;

public static class CarOwnerBookingItemDetailExtension
{
    public static CarOwnerBookingItemViewModel ToDetailViewModel(this BookingListDto dto)
    {
        if (dto == null || dto.BookingItems.Count == 0)
        {
            throw new ArgumentNullException(nameof(dto), "Booking data is required");
        }

        // Get the first booking item since we're displaying item-level details
        var bookingItem = dto.BookingItems.First();

        return new CarOwnerBookingItemViewModel
        {
            CarName = bookingItem.CarName,
            CustomerName = dto.RenterName,
            CustomerPhone = dto.RenterPhone,
            StartDate = dto.PickupDate,
            EndDate = dto.ReturnDate,
            TotalPrice = dto.TotalAmount,
            Status = bookingItem.StatusDisplay,
            CarBrand = bookingItem.CarBrand,
            CarModel = bookingItem.CarModel,
            LicensePlate = bookingItem.LicensePlate,
            PricePerDay = bookingItem.PricePerDay,
            Deposit = bookingItem.Deposit,
            SubTotal = bookingItem.SubTotal
        };
    }
}