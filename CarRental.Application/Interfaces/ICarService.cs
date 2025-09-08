using CarRental.Application.DTOs;
using CarRental.Application.DTOs.Car;
using CarRental.Domain.Models;

namespace CarRental.Application.Interfaces
{
    public interface ICarService
    {
        Task<CarDto?> GetCarByIdAsync(int id);
        Task<bool> CheckAsync(Guid? id, string licensePlate);
        Task<IEnumerable<CarDto>> GetAllCarsAsync();
        Task<IEnumerable<CarDto>> GetAvailableCarsAsync();
        Task<IEnumerable<CarDto>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CarDto>> SearchCarsAsync(CarSearchDto searchDto);
        Task<CreateCarDto> CreateCarAsync(CreateCarDto createCarDto);
        Task<PagedResult<CarSearchDto>> SearchCarsAsync(SearchFormModel searchDto, int pageNumber, int pageSize);
        Task<PagedResult<ListMyCarDto>> GetMyCarCarsAsync(bool newToOld, int pageNumber, int pageSize, Guid id);
        Task<CarDto> UpdateCarAsync(int id, CreateCarDto updateCarDto);
        Task DeleteCarAsync(int id);
        Task<CarDto> UpdateCarStatusAsync(int id, string status);
        Task<EditCarDto> GetCarDetailsAsync(Guid id);
        Task UpdateCarStatus(Guid id, string status);
        Task UpdateCarDetails(EditCarDto model);
        Task UpdateCarPricing(EditCarDto model);
        Task<CarDto> CreateCarAsync();
        Task<CarDetailsDto?> GetDetailsAsync(Guid carId, Guid? currentUserId = null, CancellationToken ct = default);

        /// <summary>
        /// Gets all cars that belong to a specific owner.
        /// Intended for scenarios like building dropdowns (no paging).
        /// </summary>
        /// <param name="ownerId">The unique identifier of the car owner.</param>
        /// <returns>A collection of <see cref="CarDto"/> representing all cars owned by the user.</returns>
        Task<IEnumerable<CarDto>> GetCarsByOwnerAsync(Guid ownerId);
    }
}