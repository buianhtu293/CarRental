using CarRental.Application.DTOs.Booking;
using CarRental.MVC.Models.Booking;

namespace CarRental.MVC.Extensions
{
    public static class BookingProcessMappingExtensions
    {
        public static BookingProcessDto ToBookingProcessDto(this BookingViewModel viewModel)
        {
            return new BookingProcessDto
            {
                SessionId = viewModel.SessionId,
                SelectedCarIds = viewModel.SelectedCarIds,
                PickupDate = viewModel.PickupDate,
                ReturnDate = viewModel.ReturnDate,
                NumberOfDays = viewModel.NumberOfDays,
                TotalAmount = viewModel.TotalAmount,
                TotalDeposit = viewModel.TotalDeposit,
                Renter = viewModel.Information.Renter.ToDto(),
                Drivers = viewModel.Information.Drivers.Select(d => d.ToDto()).ToList(),
                CarItems = viewModel.CarItems.Select(c => c.ToDto()).ToList()
            };
        }
    }
}