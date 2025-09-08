using CarRental.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Interfaces
{
    public interface IHomepageRepository : IGenericRepository<Feedback, Guid>
    {
        Task<IEnumerable<Feedback>> GetTopFeedbacksAsync(int count = 4);
        Task<IEnumerable<(string Province, int CarCount)>> GetTopProvincesWithMostCarsAsync(int count = 6);
    }

}
