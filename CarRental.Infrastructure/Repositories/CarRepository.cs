using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;
using CarRental.Domain.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace CarRental.Infrastructure.Repositories
{
	public class CarRepository : GenericRepository<Car, Guid>, ICarRepository
	{
		public CarRepository(CarRentalDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<Car>> GetAvailableCarsAsync(DateTime pickupDate, DateTime returnDate)
		{
			var unavailableCarIds = await _context.BookingItems
				.Include(bi => bi.Booking)
				.Where(bi => bi.Booking.PickupDate < returnDate &&
						   bi.Booking.ReturnDate > pickupDate &&
						   !bi.IsDeleted &&
						   !bi.Booking.IsDeleted)
				.Select(bi => bi.CarID)
				.Distinct()
				.ToListAsync();

			return await _dbSet
				.Where(c => !unavailableCarIds.Contains(c.Id) &&
						   c.Status == "Available" &&
						   !c.IsDeleted)
				.ToListAsync();
		}

		public async Task<PagedResult<Car>> GetCarsByOwnerIdAsync(Guid ownerId, bool newToOld, int pageNumber, int pageSize)
		{
			// Tạo query cơ bản
			IQueryable<Car> query = _dbSet
				.Where(c => c.OwnerID == ownerId && !c.IsDeleted);

			// Sắp xếp theo CreatedAt
			query = newToOld
				? query.OrderByDescending(c => c.CreatedAt)
				: query.OrderBy(c => c.CreatedAt);

			// Lấy tổng số bản ghi
			var totalCount = await query.CountAsync();

			// Lấy dữ liệu trang hiện tại
			var items = await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<Car>(items, totalCount, pageNumber, pageSize);
		}

		public async Task<Car?> GetByLicensePlateAsync(string licensePlate)
		{
			return await _dbSet
				.FirstOrDefaultAsync(c => c.LicensePlate == licensePlate && !c.IsDeleted);
		}

		public async Task<PagedResult<Car>> SearchAvailableCarsAsync(CarSearchCriteria criteria, int pageNumber, int pageSize)
		{
			var unavailableCarIds = await _context.BookingItems
		.Where(bi => bi.Booking.PickupDate.HasValue && bi.Booking.ReturnDate.HasValue &&
					bi.Booking.PickupDate < criteria.ReturnDateTime.Value &&
					bi.Booking.ReturnDate > criteria.PickupDateTime.Value &&
					bi.Status != BookingItemStatusEnum.Cancelled &&
					bi.Status != BookingItemStatusEnum.Completed)
		.Select(bi => bi.CarID)
		.Distinct()
		.ToListAsync();
			IQueryable<Car> query = _dbSet
				.Where(c => !c.IsDeleted &&
						   !unavailableCarIds.Contains(c.Id));

			if (!string.IsNullOrWhiteSpace(criteria.SpecificAddress))
			{
				query = query.Where(c => c.Address != null &&
										c.Address.Contains(criteria.SpecificAddress));
			}

			if (!string.IsNullOrWhiteSpace(criteria.ProvinceName) && criteria.ProvinceId != null)
			{
				query = query.Where(c => c.Province == criteria.ProvinceName);

				if (!string.IsNullOrWhiteSpace(criteria.DistrictName) && criteria.DistrictId != null)
				{
					query = query.Where(c => c.District == criteria.DistrictName);

					if (!string.IsNullOrWhiteSpace(criteria.WardName) && criteria.WardId != null)
					{
						query = query.Where(c => c.Ward == criteria.WardName);
					}
				}
			}

			var totalCount = await query.CountAsync();

			var items = await query
						.Include(doc => doc.CarDocuments)
						.OrderBy(c => c.BasePricePerDay)
						.Skip((pageNumber - 1) * pageSize)
						.Take(pageSize)
						.ToListAsync();
			return new PagedResult<Car>(items, totalCount, pageNumber, pageSize);
		}

		public async Task<bool> IsCarAvailableAsync(Guid carId, DateTime pickupDate, DateTime returnDate)
		{
			var car = await GetByIdAsync(carId);
			if (car == null || car.Status != "Available")
			{
				return false;
			}

			var hasOverlappingBookings = await _context.BookingItems
				.Include(bi => bi.Booking)
				.AnyAsync(bi => bi.CarID == carId &&
							   bi.Booking.PickupDate < returnDate &&
							   bi.Booking.ReturnDate > pickupDate &&
							   !bi.IsDeleted &&
							   !bi.Booking.IsDeleted);

			return !hasOverlappingBookings;
		}

        public async Task<IEnumerable<Car>> GetCarsByIdsAsync(List<Guid> carIds)
        {
            var allowedTypes = new[] { "Front", "Back", "Left", "Right" };

            return await _dbSet
                .Include(c => c.BookingItems)
                .Include(c => c.CarDocuments.Where(x => allowedTypes.Contains(x.DocumentType)))
                .Include(c => c.Feedbacks)
                .Where(c => carIds.Contains(c.Id) && !c.IsDeleted)
                .ToListAsync();
        }

		public async Task<decimal> GetTotalPricePerDayAsync(List<Guid> carIds)
		{
			return await _dbSet
				.Where(c => carIds.Contains(c.Id) && !c.IsDeleted)
				.SumAsync(c => c.BasePricePerDay ?? 0);
		}

		public async Task<decimal> GetTotalDepositAsync(List<Guid> carIds)
		{
			return await _dbSet
				.Where(c => carIds.Contains(c.Id) && !c.IsDeleted)
				.SumAsync(c => c.RequiredDeposit ?? 0);
		}
		public async Task<Dictionary<Guid, (double AverageRating, int TotalReviews)>> GetCarRatingsAsync(List<Guid> carIds)
		{
			var ratings = await _context.Feedbacks
				.Where(f => carIds.Contains(f.CarID) && !f.IsDeleted && f.Stars.HasValue)
				.GroupBy(f => f.CarID)
				.Select(g => new
				{
					CarId = g.Key,
					AverageRating = g.Average(f => f.Stars.Value),
					TotalReviews = g.Count()
				})
				.ToDictionaryAsync(x => x.CarId, x => (x.AverageRating, x.TotalReviews));

			return ratings;
		}

		public async Task<Dictionary<Guid, int>> GetCarRidesCountAsync(List<Guid> carIds)
		{
			var ridesCount = await _context.BookingItems
				.Include(bi => bi.Booking)
				.Where(bi => carIds.Contains(bi.CarID) &&
							!bi.IsDeleted &&
							!bi.Booking.IsDeleted &&
							bi.Booking.ReturnDate < DateTime.Now) // Only completed bookings
				.GroupBy(bi => bi.CarID)
				.ToDictionaryAsync(g => g.Key, g => g.Count());

			return ridesCount;
		}

		public async Task<Dictionary<Guid, IEnumerable<CarDocument>>> GetCarDocumentsAsync(List<Guid> carIds)
		{
			var allowedTypes = new[] { "Front", "Back", "Left", "Right" };

			var carDocuments = await _context.CarDocuments
				.Where(d => carIds.Contains(d.CarID) && allowedTypes.Contains(d.DocumentType))
				.ToListAsync();

			var result = carDocuments
				.GroupBy(d => d.CarID)
				.ToDictionary(g => g.Key, g => g.AsEnumerable());

			return result;
		}

		public async Task<Car?> GetCarWithDetailsAsync(Guid carId, CancellationToken ct = default)
		{
			return await _context.Cars
				.AsNoTracking()
				.Include(c => c.CarDocuments)
				.Include(c => c.CarSpecifications)
				.Include(c => c.BookingItems).ThenInclude(bi => bi.Booking)
				.Include(c => c.Feedbacks)
				.FirstOrDefaultAsync(c => c.Id == carId && !c.IsDeleted, ct);
		}
	}
}
