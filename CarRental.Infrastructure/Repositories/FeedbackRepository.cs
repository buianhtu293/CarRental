using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Models.Feedback;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback, Guid>, IFeedbackRepository
    {
        public FeedbackRepository(CarRentalDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<FeedbackAggregates> GetAggregatesAsync(
            FeedbackReportFilter filter,
            Guid? ownerId = null,
            bool scopeToOwner = false,
            CancellationToken ct = default)
        {
            var q = QueryWithIncludes();

            if (scopeToOwner && ownerId.HasValue)
            {
                q = q.Where(f => f.Car.OwnerID == ownerId.Value);
            }

            ApplyFilter(ref q, filter);

            var total = await q.CountAsync(ct);
            decimal avg = 0;
            if (total > 0)
            {
                avg = await q.Select(f => (decimal?)f.Stars).AverageAsync(ct) ?? 0;
                avg = Math.Round(avg, 2, MidpointRounding.AwayFromZero);
            }

            var groups = await q.GroupBy(f => f.Stars)
                .Select(g => new { Stars = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return new FeedbackAggregates
            {
                Total = total,
                AverageStars = avg,
                Count1 = groups.FirstOrDefault(g => g.Stars == 1)?.Count ?? 0,
                Count2 = groups.FirstOrDefault(g => g.Stars == 2)?.Count ?? 0,
                Count3 = groups.FirstOrDefault(g => g.Stars == 3)?.Count ?? 0,
                Count4 = groups.FirstOrDefault(g => g.Stars == 4)?.Count ?? 0,
                Count5 = groups.FirstOrDefault(g => g.Stars == 5)?.Count ?? 0
            };
        }

        /// <inheritdoc />
        public async Task<List<FeedbackReportItem>> GetPagedReportAsync(
            FeedbackReportFilter filter,
            int page,
            int pageSize,
            Guid? ownerId = null,
            bool scopeToOwner = false,
            CancellationToken ct = default)
        {
            var q = QueryWithIncludes();

            if (scopeToOwner && ownerId.HasValue)
            {
                q = q.Where(f => f.Car.OwnerID == ownerId.Value);
            }

            ApplyFilter(ref q, filter);

            var skip = Math.Max(0, (page - 1) * pageSize);

            return await q.OrderByDescending(f => f.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(f => new FeedbackReportItem
                {
                    FeedbackId = f.Id,
                    FeedbackDate = f.CreatedAt,
                    Stars = f.Stars,
                    Comment = f.Comment,
                    UserName = f.User.UserName,
                    CarId = f.CarID,
                    CarLabel = (f.Car.Brand ?? "") + " " + (f.Car.Model ?? "") + (string.IsNullOrEmpty(f.Car.LicensePlate) ? "" : $" - {f.Car.LicensePlate}"),
                    BookingNo = f.Booking.BookingNo,
                    BookingItemId = f.Booking.BookingItems.Select(bi => bi.Id).FirstOrDefault(),
                    PickupDate  = f.Booking.PickupDate,
                    ReturnDate  = f.Booking.ReturnDate,
                    CarImageUrls = f.Car.CarDocuments.Any() == true
                        ? f.Car.CarDocuments
                            .Select(x => x.FilePath ?? "/images/car_default_image.jpg")
                            .ToList()
                        : new List<string>
                        {
                            "/images/car_default_image.jpg",
                            "/images/car_default_image.jpg",
                            "/images/car_default_image.jpg",
                            "/images/car_default_image.jpg"
                        }
                })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Base query used for report operations (includes used nav props).
        /// </summary>
        private IQueryable<Feedback> QueryWithIncludes()
        {
            return _dbSet.AsNoTracking()
                .Include(f => f.User)
                .Include(f => f.Car)
                .Include(f => f.Booking)
                .Where(f => !f.IsDeleted);
        }

        /// <summary>
        /// Apply filter criteria to the queryable.
        /// </summary>
        private static void ApplyFilter(ref IQueryable<Feedback> q, FeedbackReportFilter filter)
        {
            if (filter == null) return;
            if (filter.CarId.HasValue)
            {
                q = q.Where(f => f.CarID == filter.CarId.Value);
            }

            if (filter.MinStars.HasValue)
            {
                q = q.Where(f => f.Stars >= filter.MinStars.Value);
            }

            if (filter.MaxStars.HasValue)
            {
                q = q.Where(f => f.Stars <= filter.MaxStars.Value);
            }

            if (filter.FromDate.HasValue)
            {
                q = q.Where(f => f.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                q = q.Where(f => f.CreatedAt <= filter.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                q = q.Where(f => (f.Comment ?? "").Contains(kw) || (f.User.UserName ?? "").Contains(kw));
            }

            if (filter.Stars.HasValue)
            {
                q = q.Where(f => f.Stars == filter.Stars.Value);
            }
        }
    }
}