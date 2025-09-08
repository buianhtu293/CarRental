using CarRental.Domain.Entities;
using CarRental.Domain.Models;
using System.Collections.Generic;


namespace CarRental.Domain.Interfaces
{
    public interface ICarRepository : IGenericRepository<Car, Guid>
    {
        Task<IEnumerable<Car>> GetAvailableCarsAsync(DateTime pickupDate, DateTime returnDate);
        Task<PagedResult<Car>> GetCarsByOwnerIdAsync(Guid ownerId, bool newToOld, int pageNumber, int pageSize);
        Task<Car?> GetByLicensePlateAsync(string licensePlate);
        Task<PagedResult<Car>> SearchAvailableCarsAsync(CarSearchCriteria criteria, int pageNumber, int pageSize);
        Task<bool> IsCarAvailableAsync(Guid carId, DateTime pickupDate, DateTime returnDate);
        Task<IEnumerable<Car>> GetCarsByIdsAsync(List<Guid> carIds);
        Task<decimal> GetTotalPricePerDayAsync(List<Guid> carIds);
        Task<decimal> GetTotalDepositAsync(List<Guid> carIds);
        Task<Dictionary<Guid, (double AverageRating, int TotalReviews)>> GetCarRatingsAsync(List<Guid> carIds);
        Task<Dictionary<Guid, int>> GetCarRidesCountAsync(List<Guid> carIds);
        Task<Dictionary<Guid, IEnumerable<CarDocument>>> GetCarDocumentsAsync(List<Guid> carIds);
        Task<Car?> GetCarWithDetailsAsync(Guid carId, CancellationToken ct = default);

    }
}
