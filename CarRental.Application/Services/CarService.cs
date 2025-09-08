using CarRental.Application.DTOs;
using CarRental.Application.DTOs.Car;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Application.Services
{
    public class CarService : ICarService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CarService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateCarDto> CreateCarAsync(CreateCarDto carDto)
        {
            if (carDto == null)
                throw new ArgumentNullException(nameof(carDto));

            var car = new Car
            {
                Id = Guid.NewGuid(),
                OwnerID = carDto.OwnerId,
                LicensePlate = carDto.LicensePlate,
                Brand = carDto.Brand,
                Model = carDto.Model,
                ProductionYear = carDto.ProductionYear,
                Color = carDto.Color,
                Seats = carDto.Seats,
                Transmission = carDto.Transmission,
                FuelType = carDto.FuelType,
                Mileage = carDto.Mileage,
                Description = carDto.Description,
                BasePricePerDay = carDto.BasePricePerDay,
                RequiredDeposit = carDto.RequiredDeposit,
                Status = "Available",
                Address = carDto.Address,
                Ward = carDto.Ward,
                Province = carDto.Province,
                District = carDto.District
            };

            // Lưu CarDocuments nếu có
            if (carDto.CarDocuments != null && carDto.CarDocuments.Any())
            {
                car.CarDocuments = carDto.CarDocuments.Select(d => new CarDocument
                {
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath
                }).ToList();
            }

            // Lưu CarSpecifications nếu có
            if (carDto.CarSpecifications != null && carDto.CarSpecifications.Any())
            {
                car.CarSpecifications = carDto.CarSpecifications.Select(s => new CarSpecification
                {
                    Name = s.Name,
                    Required = s.Required
                }).ToList();
            }

            await _unitOfWork.Repository<Car, Guid>().AddAsync(car);
            await _unitOfWork.SaveChangesAsync();

            // Trả về DTO
            return new CreateCarDto
            {
                Id = car.Id,
                LicensePlate = car.LicensePlate,
                Brand = car.Brand,
                Model = car.Model,
                ProductionYear = car.ProductionYear.Value,
                Color = car.Color,
                Seats = car.Seats.Value,
                Transmission = car.Transmission,
                FuelType = car.FuelType,
                BasePricePerDay = car.BasePricePerDay.Value,
                Description = car.Description,
                Status = car.Status,
            };
        }

        public async Task<bool> CheckAsync(Guid? id, string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                return false;

            return await _unitOfWork.Cars
                .AnyAsync(c => c.LicensePlate == licensePlate && (id == null || c.Id != id));
        }

        public async Task<List<CarDto>> GetMyCars(Guid id)
        {
            var carList = (await _unitOfWork.Cars.GetAllAsync())
                .Where(x => x.OwnerID == id)
                .ToList();

            return new List<CarDto>();
        }

        public Task DeleteCarAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CarDto>> GetAllCarsAsync()
        {
            var cars = await _unitOfWork.Cars.GetAllAsync();
        
            return cars.Select(car => new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model, 
                Year = car.ProductionYear ?? 0,
                Color = car.Color,
                LicensePlate = car.LicensePlate,
                PricePerDay = car.BasePricePerDay ?? 0,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                Seats = car.Seats ?? 0,
                Description = car.Description,
                Status = car.Status,
                IsAvailable = car.Status == "Available"
            }).ToList();
        }

        public Task<IEnumerable<CarDto>> GetAvailableCarsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CarDto>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<CarDto?> GetCarByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<CarSearchDto>> SearchCarsAsync(SearchFormModel searchModel, int pageNumber, int pageSize)
        {
            var criteria = new CarSearchCriteria
            {
                PickupDateTime = searchModel.CombinedPickupDateTime,
                ReturnDateTime = searchModel.CombinedReturnDateTime,
                SpecificAddress = searchModel.Address,
                ProvinceName = searchModel.ProvinceName,
                DistrictName = searchModel.DistrictName,
                WardName = searchModel.WardName,
                ProvinceId = searchModel.ProvinceId,
                DistrictId = searchModel.DistrictId,
                WardId = searchModel.WardId,
            };

            var pagedCars = await _unitOfWork.Cars.SearchAvailableCarsAsync(criteria, pageNumber, pageSize);
            var carIds = pagedCars.Items.Select(c => c.Id).ToList();

            var ratingsTask = await _unitOfWork.Cars.GetCarRatingsAsync(carIds);
            var ridesCountTask = await _unitOfWork.Cars.GetCarRidesCountAsync(carIds);


            var ratings = ratingsTask;
            var ridesCount = ridesCountTask;
            var carDtos = pagedCars.Items.Select(car =>
            {
                // Get rating info for this car
                var hasRating = ratings.TryGetValue(car.Id, out var ratingInfo);
                var hasRides = ridesCount.TryGetValue(car.Id, out var rides);

                return new CarSearchDto
                {
                    Id = car.Id,
                    Brand = car.Brand,
                    Model = car.Model,
                    Year = car.ProductionYear,
                    BasePricePerDay = car.BasePricePerDay,
                    Province = car.Province,
                    District = car.District,
                    Status = car.Status,
                    Address = car.Address,
                    Ward = car.Ward,
                    Color = car.Color,
                    Seats = car.Seats,
                    FuelType = car.FuelType,
                    Transmission = car.Transmission,
                    Description = car.Description,

					ImageUrls = car.CarDocuments
								   .Where(doc => (doc.DocumentType == "Left" || doc.DocumentType == "Right" || doc.DocumentType == "Front" || doc.DocumentType == "Back") 
                                   && !string.IsNullOrEmpty(doc.FilePath))
								   .Select(doc => doc.FilePath!)
								   .ToList(),

					AverageRating = hasRating ? ratingInfo.AverageRating : null,
                    TotalReviews = hasRating ? ratingInfo.TotalReviews : 0,

                    NumberOfRides = hasRides ? rides : 0
                };
            }).ToList();

            return new PagedResult<CarSearchDto>(carDtos, pagedCars.TotalCount, pagedCars.Page, pagedCars.PageSize);
        }

        public Task<CarDto> UpdateCarAsync(int id, CreateCarDto updateCarDto)
        {
            throw new NotImplementedException();
        }

        public Task<CarDto> UpdateCarStatusAsync(int id, string status)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CarDto>> SearchCarsAsync(CarSearchDto searchDto)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<ListMyCarDto>> GetMyCarCarsAsync(bool newToOld, int pageNumber, int pageSize, Guid id)
        {
            var pagedCars = await _unitOfWork.Cars.GetCarsByOwnerIdAsync(id,//new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), 
                newToOld, pageNumber, pageSize);
            var carIds = pagedCars.Items.Select(c => c.Id).ToList();

            var ratingsTask = await _unitOfWork.Cars.GetCarRatingsAsync(carIds);
            var ridesCountTask = await _unitOfWork.Cars.GetCarRidesCountAsync(carIds);
            var carDocsTask = await _unitOfWork.Cars.GetCarDocumentsAsync(carIds);

            var ratings = ratingsTask;
            var ridesCount = ridesCountTask;
            var carDocs = carDocsTask;

            var carDtos = pagedCars.Items.Select(car =>
            {
                var hasRating = ratings.TryGetValue(car.Id, out var ratingInfo);
                var hasRides = ridesCount.TryGetValue(car.Id, out var rides);
                var documents = carDocs.TryGetValue(car.Id, out var docs)
                     ? docs
                    .Select(d => new DTOs.CarDocumentDto
                    {
                        DocumentType = d.DocumentType,
                        FilePath = d.FilePath
                    })
                    .ToList()
                : new List<DTOs.CarDocumentDto>();

                return new ListMyCarDto
                {
                    Id = car.Id,
                    Brand = car.Brand,
                    Model = car.Model,
                    Year = car.ProductionYear,
                    BasePricePerDay = car.BasePricePerDay,
                    Province = car.Province,
                    District = car.District,
                    Status = car.Status,
                    Address = car.Address,
                    Ward = car.Ward,
                    Color = car.Color,
                    Seats = car.Seats,
                    FuelType = car.FuelType,
                    Transmission = car.Transmission,
                    Description = car.Description,

                    AverageRating = hasRating ? ratingInfo.AverageRating : null,
                    TotalReviews = hasRating ? ratingInfo.TotalReviews : 0,

                    NumberOfRides = hasRides ? rides : 0,

                    CarDocuments = documents
                };
            }).ToList();

            return new PagedResult<ListMyCarDto>(carDtos, pagedCars.TotalCount, pagedCars.Page, pagedCars.PageSize);
        }

        public async Task<Car?> GetCarWithDetailsAsync(Guid id)
        {
            return await _unitOfWork.Cars.GetByIdAsync(
                id,
                null,
                c => c.CarDocuments,
                c => c.CarSpecifications
            );
        }

        public async Task<EditCarDto> GetCarDetailsAsync(Guid id)
        {
            var car = await GetCarWithDetailsAsync(id);

            if (car == null)
            {
                return null;
            }

            var ratings = await _unitOfWork.Cars.GetCarRatingsAsync(new List<Guid> { id });
            var ridesCount = await _unitOfWork.Cars.GetCarRidesCountAsync(new List<Guid> { id });

            var hasRating = ratings.TryGetValue(id, out var ratingInfo);
            var hasRides = ridesCount.TryGetValue(id, out var rides);

            return new EditCarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.ProductionYear,
                BasePricePerDay = car.BasePricePerDay,
                Province = car.Province,
                District = car.District,
                Status = car.Status,
                Address = car.Address,
                Ward = car.Ward,
                Color = car.Color,
                Seats = car.Seats,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                Description = car.Description,
                Mileage = car.Mileage,
                FuelConsumption = car.FuelConsumption,
                RequiredDeposit = car.RequiredDeposit,

                AverageRating = hasRating ? ratingInfo.AverageRating : null,
                TotalReviews = hasRating ? ratingInfo.TotalReviews : 0,

                NumberOfRides = hasRides ? rides : 0,

                CarDocuments = car.CarDocuments.Select(d => new DTOs.CarDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath
                }).ToList(),
                CarSpecifications = car.CarSpecifications.Select(d => new DTOs.CarSpecificationDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Required = d.Required
                }).ToList()
            };
        }

        public async Task UpdateCarStatus(Guid id, string status)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car != null)
            {
                car.Status = status;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpdateCarDetails(EditCarDto model)
        {
            var car = await GetCarWithDetailsAsync(model.Id);
            if (car == null) return;

            // update fields
            car.Mileage = model.Mileage;
            car.FuelConsumption = model.FuelConsumption;
            car.Province = model.Province;
            car.District = model.District;
            car.Ward = model.Ward;
            car.Address = model.Address;
            car.Description = model.Description;

            // ==== Specifications ====
            var allSpecs = new[] { "Bluetooth", "GPS", "Camera", "Sunroof", "ChildLock", "ChildSeat", "DVD", "USB" };

            // specs từ form
            var specsFromForm = model.CarSpecifications?.ToDictionary(s => s.Name, s => s.Required)
                                ?? new Dictionary<string, bool>();

            foreach (var specName in allSpecs)
            {
                var existing = car.CarSpecifications.FirstOrDefault(s => s.Name == specName);

                if (specsFromForm.ContainsKey(specName))
                {
                    // nếu đã có thì update, chưa có thì add
                    if (existing != null)
                    {
                        existing.Required = specsFromForm[specName];
                    }
                    else
                    {
                        car.CarSpecifications.Add(new CarSpecification
                        {
                            Id = Guid.NewGuid(),
                            Name = specName,
                            Required = specsFromForm[specName]
                        });
                    }
                }
                else
                {
                    // Nếu không còn trong form thì set Required = false
                    if (existing != null)
                    {
                        existing.Required = false;
                    }
                }
            }

            // ==== Documents ====
            var docs = new[] { "Front", "Back", "Left", "Right" };

            foreach (var type in docs)
            {
                var docFromForm = model.CarDocuments?.FirstOrDefault(d => d.DocumentType == type);
                var existingDoc = car.CarDocuments.FirstOrDefault(d => d.DocumentType == type);

                if (docFromForm != null && !string.IsNullOrEmpty(docFromForm.FilePath))
                {
                    if (existingDoc != null)
                    {
                        existingDoc.FilePath = docFromForm.FilePath; // update file
                    }
                    else
                    {
                        car.CarDocuments.Add(new CarDocument
                        {
                            Id = Guid.NewGuid(),
                            DocumentType = type,
                            FilePath = docFromForm.FilePath
                        });
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCarPricing(EditCarDto model)
        {
            var car = await GetCarWithDetailsAsync(model.Id);
            if (car == null) return;

            // update pricing fields
            car.BasePricePerDay = model.BasePricePerDay;
            car.RequiredDeposit = model.RequiredDeposit;

            // ==== Terms ====
            var allTerms = new[] { "NoSmoking", "NoPet", "NoFood", "Other" };

            // map từ form: TermName -> Required
            var termsFromForm = model.CarSpecifications?.ToDictionary(t => t.Name, t => t.Required)
                               ?? new Dictionary<string, bool>();

            foreach (var termName in allTerms)
            {
                var existing = car.CarSpecifications.FirstOrDefault(t => t.Name == termName);

                if (termsFromForm.ContainsKey(termName))
                {
                    // update hoặc add
                    if (existing != null)
                    {
                        existing.Required = termsFromForm[termName];
                    }
                    else
                    {
                        car.CarSpecifications.Add(new CarSpecification
                        {
                            Id = Guid.NewGuid(),
                            Name = termName,
                            Required = termsFromForm[termName]
                        });
                    }
                }
                else
                {
                    // nếu form không gửi → mặc định set Required = false (chứ không xoá)
                    if (existing != null)
                    {
                        existing.Required = false;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

		public async Task<CarDetailsDto?> GetDetailsAsync(Guid carId, Guid? currentUserId = null, CancellationToken ct = default)
		{
			var car = await _unitOfWork.Cars.GetCarWithDetailsAsync(carId, ct);
			if (car == null) return null;

			var ratingDict = await _unitOfWork.Cars.GetCarRatingsAsync(new List<Guid> { carId });
			ratingDict.TryGetValue(carId, out var rating);
			var (avg, total) = rating;

			bool isBookedByUser = false;
			if (currentUserId.HasValue)
			{
				isBookedByUser = car.BookingItems
					.Any(bi => bi.Booking.RenterID == currentUserId.Value);
			}

            var dto = new CarDetailsDto
            {
                Header = new CarHeaderDto
                {
                    CarId = car.Id,
                    DisplayName = $"{car.Brand} {car.Model} {car.ProductionYear}".Trim(),
                    AverageRating = Math.Round(avg, 1),
                    TotalReviews = total,
                    LocationText = string.Join(", ", new[] { car.District, car.Province }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    IsBookedByCurrentUser = isBookedByUser
                },
                Basic = new CarBasicInfoDto
                {
                    LicensePlate = car.LicensePlate,
                    Brand = car.Brand,
                    Model = car.Model,
                    ProductionYear = car.ProductionYear,
                    Color = car.Color,
                    Seats = car.Seats,
                    Transmission = car.Transmission,
                    FuelType = car.FuelType,
                    Documents = car.CarDocuments
                        .Select(d => new DTOs.Car.CarDocumentDto
                        {
                            Id = d.Id,
                            DocumentType = d.DocumentType,
                            //FilePath = isBookedByUser ? d.FilePath : null,
                            FilePath = d.FilePath
                        })
                        .ToList()
                },
                Details = new CarDetailInfoDto
                {
                    Mileage = car.Mileage,
                    FuelConsumption = car.FuelConsumption,
                    Description = car.Description,
                    Specifications = car.CarSpecifications
                        .OrderBy(s => s.Name)
                        .Select(s => new DTOs.Car.CarSpecificationDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Required = s.Required
                        })
                        .ToList()
                },
                Terms = new CarTermsDto
                {
                    BasePricePerDay = car.BasePricePerDay,
                    RequiredDeposit = car.RequiredDeposit,
                    Status = car.Status
                }
            };

			return dto;
		}

		public Task<CarDto> CreateCarAsync()
		{
			throw new NotImplementedException();
		}
        
        /// <inheritdoc/>
        public async Task<IEnumerable<CarDto>> GetCarsByOwnerAsync(Guid ownerId)
        {
            // Reuse repository query but bypass pagination
            var pagedCars = await _unitOfWork.Cars.GetCarsByOwnerIdAsync(
                ownerId,
                newToOld: false,
                pageNumber: 1,
                pageSize: int.MaxValue);

            return pagedCars.Items.Select(car => new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                LicensePlate = car.LicensePlate,
                Year = car.ProductionYear ?? 0,
                Color = car.Color,
                Seats = car.Seats ?? 0,
                Transmission = car.Transmission,
                FuelType = car.FuelType,
                PricePerDay = car.BasePricePerDay ?? 0,
                Status = car.Status
            }).ToList();
        }
	}
}
