using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Repositories
{
    public class HomepageRepository : GenericRepository<Feedback, Guid>, IHomepageRepository
    {
        public HomepageRepository(CarRentalDbContext context) : base(context)
        {

        }
        public async Task<IEnumerable<Feedback>> GetTopFeedbacksAsync(int count = 4)
        {
            return await _context.Feedbacks
                .Include(f => f.Booking) // Tải dữ liệu từ bảng Booking liên quan
                .Include(f => f.User)   // Tải dữ liệu User để lấy tên
                .Where(f => f.Stars == 5 && !string.IsNullOrEmpty(f.Comment))
                // Sắp xếp giảm dần theo ngày trả xe của booking
                .OrderByDescending(f => f.Booking.ReturnDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<(string Province, int CarCount)>> GetTopProvincesWithMostCarsAsync(int count = 6)
        {
            var queryResult = await _context.Cars
                    .Where(c => !c.IsDeleted && c.Status == "Available" && !string.IsNullOrEmpty(c.Province))
                    .GroupBy(c => c.Province!)
                    .Select(g => new { Province = g.Key, CarCount = g.Count() })
                    .OrderByDescending(x => x.CarCount)
                    .Take(count)
                    .ToListAsync();
            return queryResult.Select(x => (x.Province, x.CarCount));

        }
    }
}
