using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application.DTOs.Booking;
using CarRental.Application.DTOs.CarOwnerBooking;
using CarRental.Application.Interfaces;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Application.Services
{
	public class CarOwnerBookingService : ICarOwnerBookingService
	{
		private readonly IUnitOfWork _unitOfWork;

		public CarOwnerBookingService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BookingListResponseDto> GetBookingListAsync(BookingListRequestDto request)
		{
			var pagedResult = await _unitOfWork.CarOwnerBookings.GetBookingsPagedAsync(request.Page, request.PageSize, request.SortBy, request.StatusFilter, request.SearchTerm, request.UserId);

			var allowedTypes = new[] { "Front", "Back", "Left", "Right" };

			// Map từ Domain sang Application DTO
			var result = new BookingListResponseDto
			{
				Bookings = pagedResult.Items.ToList().Select(booking => new BookingListDto
				{
					Id = booking.Id,
					BookingNumber = booking.BookingNo ?? string.Empty,
					BookingDate = booking.CreatedAt,
					PickupDate = booking.PickupDate ?? DateTime.MinValue,
					ReturnDate = booking.ReturnDate ?? DateTime.MinValue,
					NumberOfDays = CalculateNumberOfDays(booking.PickupDate ?? DateTime.MinValue, booking.ReturnDate ?? DateTime.MinValue),
					TotalAmount = booking.TotalAmount ?? 0m,
					TotalDeposit = booking.BookingItems?.Sum(bi => bi.Deposit ?? 0m) ?? 0m,
					RenterName = booking.Renter?.FullName ?? string.Empty,
					RenterEmail = booking.Renter?.Email ?? string.Empty,
					RenterPhone = booking.Renter?.PhoneNumber ?? string.Empty,
					BookingItems = booking.BookingItems?.Select(bookingItem => new BookingItemListDto
					{
						Id = bookingItem.Id,
						BookingId = bookingItem.BookingID,
						CarId = bookingItem.CarID,
						CarBrand = bookingItem.Car?.Brand ?? string.Empty,
						CarModel = bookingItem.Car?.Model ?? string.Empty,
						LicensePlate = bookingItem.Car?.LicensePlate ?? string.Empty,
						PricePerDay = bookingItem.PricePerDay ?? 0m,
						Deposit = bookingItem.Deposit ?? 0m,
						SubTotal = (bookingItem.PricePerDay ?? 0m) * CalculateNumberOfDays(booking.PickupDate ?? DateTime.MinValue, booking.ReturnDate ?? DateTime.MinValue),
						Status = bookingItem.Status,
						CarImages = bookingItem.Car?.CarDocuments?
							.Where(doc => allowedTypes.Contains(doc.DocumentType))
							.Select(doc => doc.FilePath)
							.ToList() ?? new List<string>()
					}).ToList() ?? new List<BookingItemListDto>()
				}).ToList(),
				TotalCount = pagedResult.TotalCount,
				CurrentPage = pagedResult.Page,
				PageSize = pagedResult.PageSize
			};

			return result;
		}

		/// <inheritdoc />
		public async Task<bool> ConfirmDepositAsync(Guid bookingItemId, Guid ownerId)
		{
			// Load booking item + Car to verify ownership in a single round-trip.
			var bookingItem = await _unitOfWork.BookingItems.GetFirstOrDefaultAsync(
				bi => bi.Id == bookingItemId && !bi.IsDeleted,
				null,
				x => x.Car,
				x => x.Booking
			);

			if (bookingItem is null)
			{
				return false;
			}

			// Authorize: only the owner of the car in this booking item can confirm
			if (bookingItem.Car.OwnerID != ownerId)
			{
				return false;
			}

			// Business rule: can only confirm from PendingDeposit
			if (bookingItem.Status != BookingItemStatusEnum.PendingDeposit)
			{
				return false;
			}

			// Update status
			bookingItem.Status = BookingItemStatusEnum.Confirm;
			bookingItem.UpdatedAt = DateTime.UtcNow;

			// Persist atomically
			await _unitOfWork.BookingItems.UpdateAsync(bookingItem);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		/// <inheritdoc />
		public async Task<bool> ConfirmPaymentAsync(Guid bookingItemId, Guid ownerId)
		{
			var bookingItem = await _unitOfWork.BookingItems.GetFirstOrDefaultAsync(
				bi => bi.Id == bookingItemId && !bi.IsDeleted,
				null,
				x => x.Car,
				x => x.Booking
			);

			if (bookingItem is null)
			{
				return false;
			}

			if (bookingItem.Car.OwnerID != ownerId)
			{
				return false;
			}

			// Business rule: must be PendingPayment before confirming
			if (bookingItem.Status != BookingItemStatusEnum.PendingPayment)
			{
				return false;
			}

			bookingItem.Status = BookingItemStatusEnum.Completed;
			bookingItem.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.BookingItems.UpdateAsync(bookingItem);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		/// <inheritdoc />
        public async Task<BookingListResponseDto> GetBookingListDetailAsync(Guid ownerId, Guid bookingItemId)
        {
            var pagedResult = await _unitOfWork.CarOwnerBookings.GetBookingsPagedAsync(
                1,
                int.MaxValue,
                "newest",
                null,
                string.Empty,
                ownerId
            );

            var allowedTypes = new[] { "Front", "Back", "Left", "Right" };

            // Filter to only include the requested booking item
            var filteredBookings = pagedResult.Items.Where(b =>
                b.BookingItems.Any(bi => bi.Id == bookingItemId)
            ).AsQueryable();

            // Map domain entities to DTOs
            var result = new BookingListResponseDto
            {
                Bookings = filteredBookings.AsEnumerable().Select(booking => new BookingListDto
                {
                    Id = booking.Id,
                    BookingNumber = booking.BookingNo ?? string.Empty,
                    BookingDate = booking.CreatedAt,
                    PickupDate = booking.PickupDate ?? DateTime.MinValue,
                    ReturnDate = booking.ReturnDate ?? DateTime.MinValue,
                    NumberOfDays = CalculateNumberOfDays(
                        booking.PickupDate ?? DateTime.MinValue,
                        booking.ReturnDate ?? DateTime.MinValue),
                    TotalAmount = booking.TotalAmount ?? 0m,
                    TotalDeposit = booking.BookingItems?.Sum(bi => bi.Deposit ?? 0m) ?? 0m,

                    RenterName = booking.Renter?.FullName ?? string.Empty,
                    RenterEmail = booking.Renter?.Email ?? string.Empty,
                    RenterPhone = booking.Renter?.PhoneNumber ?? string.Empty,

                    BookingItems = booking.BookingItems?
                        .Where(bi => bi.Id == bookingItemId)
                        .Select(bookingItem => new BookingItemListDto
                        {
                            Id = bookingItem.Id,
                            BookingId = bookingItem.BookingID,
                            CarId = bookingItem.CarID,
                            CarBrand = bookingItem.Car?.Brand ?? string.Empty,
                            CarModel = bookingItem.Car?.Model ?? string.Empty,
                            LicensePlate = bookingItem.Car?.LicensePlate ?? string.Empty,
                            PricePerDay = bookingItem.PricePerDay ?? 0m,
                            Deposit = bookingItem.Deposit ?? 0m,
                            SubTotal = (bookingItem.PricePerDay ?? 0m) * CalculateNumberOfDays(
                                booking.PickupDate ?? DateTime.MinValue,
                                booking.ReturnDate ?? DateTime.MinValue),
                            Status = bookingItem.Status,
                            CarImages = bookingItem.Car?.CarDocuments?
                                .Where(doc => allowedTypes.Contains(doc.DocumentType))
                                .Select(doc => doc.FilePath)
                                .ToList() ?? new List<string>()
                        }).ToList() ?? new List<BookingItemListDto>()
                }).ToList(),

                TotalCount = filteredBookings.Count(),
                CurrentPage = 1,
                PageSize = 1
            };

            return result;
        }

		private static decimal CalculateNumberOfDays(DateTime pickupDate, DateTime returnDate)
		{
			if (pickupDate == DateTime.MinValue || returnDate == DateTime.MinValue)
				return 0;

			var days = ((decimal)((returnDate - pickupDate).TotalHours) / 24);
			return Math.Max(0, days);
		}
	}
}