using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application.DTOs.Booking;
using CarRental.Application.DTOs.CarOwnerBooking;

namespace CarRental.Application.Interfaces
{
    public interface ICarOwnerBookingService
    {
        Task<BookingListResponseDto> GetBookingListAsync(BookingListRequestDto request);

        /// <summary>
        /// Confirm that the deposit for a specific booking item has been received.
        /// Only the car owner of the car in that booking item can perform this.
        /// </summary>
        /// <param name="bookingItemId">Booking item id.</param>
        /// <param name="ownerId">Current owner id (must match the car owner).</param>
        /// <returns>true if status updated; otherwise false.</returns>
        Task<bool> ConfirmDepositAsync(Guid bookingItemId, Guid ownerId);

        /// <summary>
        /// Confirm that the full payment for a specific booking item has been received.
        /// Only the car owner of the car in that booking item can perform this.
        /// </summary>
        /// <param name="bookingItemId">Booking item id.</param>
        /// <param name="ownerId">Current owner id (must match the car owner).</param>
        /// <returns>true if status updated; otherwise false.</returns>
        Task<bool> ConfirmPaymentAsync(Guid bookingItemId, Guid ownerId);

        /// <summary>
        /// Retrieves detailed information about a specific booking item for the car owner.
        /// </summary>
        /// <param name="ownerId">The unique identifier for the car owner requesting the booking details.</param>
        /// <param name="bookingItemId">The unique identifier for the booking item whose details are to be retrieved.</param>
        /// <returns>A task representing the asynchronous operation, with a <see cref="BookingListResponseDto"/> containing the details of the specified booking item.</returns>
        Task<BookingListResponseDto> GetBookingListDetailAsync(Guid ownerId, Guid bookingItemId);
    }
}