using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace CarRental.Infrastructure.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();


            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed in order: Roles -> Users -> Brands -> Cars -> Wallets -> Sample Data
                await SeedRolesAsync(roleManager, logger);
                var (adminUser, carOwners, customers) = await SeedUsersAsync(userManager, logger);
                await SeedBrandsAsync(context, logger);
                var cars = await SeedCarsAsync(context, carOwners, logger);
                await SeedWalletsAsync(context, customers, logger);
                await SeedSampleBookingsAsync(context, customers, cars, logger);

                await context.SaveChangesAsync();
                logger.LogInformation("Database seeded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger<SeedData> logger)
        {
            var roles = new[] { "Administrator", "CarOwner", "Customer" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Created role: {RoleName}", roleName);
                    }
                    else
                    {
                        logger.LogError("Failed to create role: {RoleName}. Errors: {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Role {RoleName} already exists", roleName);
                }
            }
        }

        private static async Task<(User admin, List<User> carOwners, List<User> customers)> SeedUsersAsync(
            UserManager<User> userManager, ILogger<SeedData> logger)
        {
            var carOwners = new List<User>();
            var customers = new List<User>();
            User? adminUser = null;

            // Seed Administrator
            var adminEmail = "admin@carrental.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    PhoneNumber = "0999999999",
                    PhoneNumberConfirmed = true,
                    Address = "Admin Office, District 1",
                    Province = "Ho Chi Minh City",
                    District = "District 1",
                    Ward = "Ben Nghe Ward",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                    logger.LogInformation("Created admin user: {Email}", adminEmail);
                }
            }
            else
            {
                adminUser = existingAdmin;
                logger.LogInformation("Admin user {Email} already exists", adminEmail);
            }

            // Seed Car Owners
            var carOwnerData = new[]
            {
                new { Email = "owner1@carrental.com", FullName = "Nguyen Van An", Phone = "0901234567", Address = "123 Nguyen Van Linh", District = "District 7", Ward = "Tan Phong Ward" },
                new { Email = "owner2@carrental.com", FullName = "Tran Thi Binh", Phone = "0902345678", Address = "456 Le Van Viet", District = "District 9", Ward = "Tang Nhon Phu A Ward" },
                new { Email = "owner3@carrental.com", FullName = "Le Van Cuong", Phone = "0903456789", Address = "789 Vo Van Tan", District = "District 3", Ward = "Vo Thi Sau Ward" },
                new { Email = "owner4@carrental.com", FullName = "Pham Thi Dung", Phone = "0904567890", Address = "321 Pham Van Dong", District = "Thu Duc City", Ward = "Linh Trung Ward" },
                new { Email = "owner5@carrental.com", FullName = "Hoang Van Em", Phone = "0905678901", Address = "654 Nguyen Thi Minh Khai", District = "District 1", Ward = "Ben Thanh Ward" }
            };

            foreach (var ownerInfo in carOwnerData)
            {
                var existingOwner = await userManager.FindByEmailAsync(ownerInfo.Email);
                if (existingOwner == null)
                {
                    var owner = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = ownerInfo.Email,
                        Email = ownerInfo.Email,
                        EmailConfirmed = true,
                        FullName = ownerInfo.FullName,
                        PhoneNumber = ownerInfo.Phone,
                        PhoneNumberConfirmed = true,
                        Address = ownerInfo.Address,
                        Province = "Ho Chi Minh City",
                        District = ownerInfo.District,
                        Ward = ownerInfo.Ward,
                        LicenseId = $"B2-{Random.Shared.Next(100000000, 999999999)}",
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(owner, "Owner@123456");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(owner, "CarOwner");
                        carOwners.Add(owner);
                        logger.LogInformation("Created car owner: {Email}", ownerInfo.Email);
                    }
                    else
                    {
                        logger.LogError("Failed to create car owner: {Email}. Errors: {Errors}",
                            ownerInfo.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    carOwners.Add(existingOwner);
                    logger.LogInformation("Car owner {Email} already exists", ownerInfo.Email);
                }
            }

            // Seed Customers
            var customerData = new[]
            {
                new { Email = "customer1@carrental.com", FullName = "Pham Minh Duc", Phone = "0904567890", Address = "321 Cach Mang Thang 8", District = "District 10", Ward = "Ward 6" },
                new { Email = "customer2@carrental.com", FullName = "Hoang Thi Lan", Phone = "0905678901", Address = "654 Nguyen Thi Minh Khai", District = "District 1", Ward = "Ben Thanh Ward" },
                new { Email = "customer3@carrental.com", FullName = "Dang Van Hung", Phone = "0906789012", Address = "987 Ly Thuong Kiet", District = "District 11", Ward = "Ward 10" },
                new { Email = "customer4@carrental.com", FullName = "Vu Thi Mai", Phone = "0907890123", Address = "147 Tran Hung Dao", District = "District 5", Ward = "Ward 11" },
                new { Email = "customer5@carrental.com", FullName = "Bui Van Nam", Phone = "0908901234", Address = "258 Le Lai", District = "District 1", Ward = "Ben Thanh Ward" },
                new { Email = "customer6@carrental.com", FullName = "Do Thi Oanh", Phone = "0909012345", Address = "369 Hai Ba Trung", District = "District 3", Ward = "Vo Thi Sau Ward" }
            };

            foreach (var customerInfo in customerData)
            {
                var existingCustomer = await userManager.FindByEmailAsync(customerInfo.Email);
                if (existingCustomer == null)
                {
                    var customer = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = customerInfo.Email,
                        Email = customerInfo.Email,
                        EmailConfirmed = true,
                        FullName = customerInfo.FullName,
                        PhoneNumber = customerInfo.Phone,
                        PhoneNumberConfirmed = true,
                        Address = customerInfo.Address,
                        Province = "Ho Chi Minh City",
                        District = customerInfo.District,
                        Ward = customerInfo.Ward,
                        LicenseId = $"B2-{Random.Shared.Next(100000000, 999999999)}",
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(customer, "Customer@123456");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(customer, "Customer");
                        customers.Add(customer);
                        logger.LogInformation("Created customer: {Email}", customerInfo.Email);
                    }
                    else
                    {
                        logger.LogError("Failed to create customer: {Email}. Errors: {Errors}",
                            customerInfo.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    customers.Add(existingCustomer);
                    logger.LogInformation("Customer {Email} already exists", customerInfo.Email);
                }
            }

            return (adminUser!, carOwners, customers);
        }

        private static async Task SeedBrandsAsync(CarRentalDbContext context, ILogger<SeedData> logger)
        {
            var brandModels = new[]
            {
                // Toyota
                new { BrandName = "Toyota", ModelName = "Vios" },
                new { BrandName = "Toyota", ModelName = "Camry" },
                new { BrandName = "Toyota", ModelName = "Innova" },
                new { BrandName = "Toyota", ModelName = "Fortuner" },
                new { BrandName = "Toyota", ModelName = "Corolla Cross" },
                
                // Honda
                new { BrandName = "Honda", ModelName = "City" },
                new { BrandName = "Honda", ModelName = "Civic" },
                new { BrandName = "Honda", ModelName = "CR-V" },
                new { BrandName = "Honda", ModelName = "Accord" },
                new { BrandName = "Honda", ModelName = "HR-V" },
                
                // Mazda
                new { BrandName = "Mazda", ModelName = "3" },
                new { BrandName = "Mazda", ModelName = "6" },
                new { BrandName = "Mazda", ModelName = "CX-5" },
                new { BrandName = "Mazda", ModelName = "CX-8" },
                
                // Ford
                new { BrandName = "Ford", ModelName = "EcoSport" },
                new { BrandName = "Ford", ModelName = "Focus" },
                new { BrandName = "Ford", ModelName = "Ranger" },
                new { BrandName = "Ford", ModelName = "Everest" },
                
                // Hyundai
                new { BrandName = "Hyundai", ModelName = "Accent" },
                new { BrandName = "Hyundai", ModelName = "Elantra" },
                new { BrandName = "Hyundai", ModelName = "Tucson" },
                new { BrandName = "Hyundai", ModelName = "Santa Fe" },
                
                // Kia
                new { BrandName = "Kia", ModelName = "Morning" },
                new { BrandName = "Kia", ModelName = "Cerato" },
                new { BrandName = "Kia", ModelName = "Seltos" },
                new { BrandName = "Kia", ModelName = "Sorento" },
                
                // VinFast
                new { BrandName = "VinFast", ModelName = "Fadil" },
                new { BrandName = "VinFast", ModelName = "Lux A2.0" },
                new { BrandName = "VinFast", ModelName = "Lux SA2.0" },
                new { BrandName = "VinFast", ModelName = "VF e34" }
            };

            foreach (var brandModel in brandModels)
            {
                var existingBrand = await context.Brands
                    .FirstOrDefaultAsync(b => b.BrandName == brandModel.BrandName && 
                                            b.ModelName == brandModel.ModelName && 
                                            !b.IsDeleted);

                if (existingBrand == null)
                {
                    var brand = new Brand
                    {
                        Id = Guid.NewGuid(),
                        BrandName = brandModel.BrandName,
                        ModelName = brandModel.ModelName,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    context.Brands.Add(brand);
                    logger.LogInformation("Created brand-model: {Brand} {Model}", brandModel.BrandName, brandModel.ModelName);
                }
                else
                {
                    logger.LogInformation("Brand-model {Brand} {Model} already exists", brandModel.BrandName, brandModel.ModelName);
                }
            }
        }

        private static async Task<List<Car>> SeedCarsAsync(CarRentalDbContext context, List<User> carOwners, ILogger<SeedData> logger)
        {
            var cars = new List<Car>();

            if (!carOwners.Any())
            {
                logger.LogWarning("No car owners found to assign cars");
                return cars;
            }

            var carData = new[]
            {
                // Toyota vehicles
                new { Brand = "Toyota", Model = "Vios", LicensePlate = "30A-12345", Color = "White", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 600000m, Deposit = 3500000m, Year = 2022 },
                new { Brand = "Toyota", Model = "Camry", LicensePlate = "30A-12346", Color = "Silver", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 1000000m, Deposit = 6000000m, Year = 2021 },
                new { Brand = "Toyota", Model = "Innova", LicensePlate = "30A-12347", Color = "Black", Seats = 7, Transmission = "Manual", FuelType = "Gasoline", Price = 900000m, Deposit = 5000000m, Year = 2020 },
                new { Brand = "Toyota", Model = "Fortuner", LicensePlate = "30A-12348", Color = "White", Seats = 7, Transmission = "Automatic", FuelType = "Diesel", Price = 1500000m, Deposit = 8000000m, Year = 2023 },
                new { Brand = "Toyota", Model = "Corolla Cross", LicensePlate = "30A-12349", Color = "Red", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 800000m, Deposit = 4500000m, Year = 2022 },

                // Honda vehicles  
                new { Brand = "Honda", Model = "City", LicensePlate = "30A-12350", Color = "Blue", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 650000m, Deposit = 3500000m, Year = 2021 },
                new { Brand = "Honda", Model = "Civic", LicensePlate = "30A-12351", Color = "Gray", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 800000m, Deposit = 4500000m, Year = 2020 },
                new { Brand = "Honda", Model = "CR-V", LicensePlate = "30A-12352", Color = "White", Seats = 7, Transmission = "Automatic", FuelType = "Gasoline", Price = 1200000m, Deposit = 6500000m, Year = 2022 },
                new { Brand = "Honda", Model = "Accord", LicensePlate = "30A-12353", Color = "Black", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 1100000m, Deposit = 6000000m, Year = 2021 },

                // Mazda vehicles
                new { Brand = "Mazda", Model = "3", LicensePlate = "30A-12354", Color = "Red", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 750000m, Deposit = 4000000m, Year = 2021 },
                new { Brand = "Mazda", Model = "CX-5", LicensePlate = "30A-12355", Color = "Silver", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 950000m, Deposit = 5000000m, Year = 2022 },
                new { Brand = "Mazda", Model = "6", LicensePlate = "30A-12356", Color = "Blue", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 900000m, Deposit = 5000000m, Year = 2020 },

                // Ford vehicles
                new { Brand = "Ford", Model = "EcoSport", LicensePlate = "30A-12357", Color = "Orange", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 700000m, Deposit = 4000000m, Year = 2021 },
                new { Brand = "Ford", Model = "Ranger", LicensePlate = "30A-12358", Color = "Green", Seats = 5, Transmission = "Manual", FuelType = "Diesel", Price = 1200000m, Deposit = 7000000m, Year = 2022 },
                new { Brand = "Ford", Model = "Everest", LicensePlate = "30A-12359", Color = "Black", Seats = 7, Transmission = "Automatic", FuelType = "Diesel", Price = 1400000m, Deposit = 7500000m, Year = 2021 },

                // Hyundai vehicles
                new { Brand = "Hyundai", Model = "Accent", LicensePlate = "30A-12360", Color = "Silver", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 550000m, Deposit = 3000000m, Year = 2022 },
                new { Brand = "Hyundai", Model = "Tucson", LicensePlate = "30A-12361", Color = "White", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 1000000m, Deposit = 5500000m, Year = 2021 },
                new { Brand = "Hyundai", Model = "Santa Fe", LicensePlate = "30A-12362", Color = "Black", Seats = 7, Transmission = "Automatic", FuelType = "Gasoline", Price = 1300000m, Deposit = 7000000m, Year = 2022 },

                // Kia vehicles
                new { Brand = "Kia", Model = "Morning", LicensePlate = "30A-12363", Color = "Yellow", Seats = 4, Transmission = "Manual", FuelType = "Gasoline", Price = 450000m, Deposit = 2500000m, Year = 2021 },
                new { Brand = "Kia", Model = "Cerato", LicensePlate = "30A-12364", Color = "White", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 700000m, Deposit = 3500000m, Year = 2020 },
                new { Brand = "Kia", Model = "Seltos", LicensePlate = "30A-12365", Color = "Red", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 850000m, Deposit = 4500000m, Year = 2022 },

                // VinFast vehicles
                new { Brand = "VinFast", Model = "Fadil", LicensePlate = "30A-12366", Color = "Blue", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 550000m, Deposit = 3000000m, Year = 2021 },
                new { Brand = "VinFast", Model = "Lux A2.0", LicensePlate = "30A-12367", Color = "Black", Seats = 5, Transmission = "Automatic", FuelType = "Gasoline", Price = 1200000m, Deposit = 7000000m, Year = 2022 }
            };

            var ownerIndex = 0;
            foreach (var carInfo in carData)
            {
                var existingCar = await context.Cars
                    .FirstOrDefaultAsync(c => c.LicensePlate == carInfo.LicensePlate && !c.IsDeleted);

                if (existingCar == null)
                {
                    var car = new Car
                    {
                        Id = Guid.NewGuid(),
                        OwnerID = carOwners[ownerIndex % carOwners.Count].Id,
                        Brand = carInfo.Brand,
                        Model = carInfo.Model,
                        LicensePlate = carInfo.LicensePlate,
                        Color = carInfo.Color,
                        Seats = carInfo.Seats,
                        Transmission = carInfo.Transmission,
                        FuelType = carInfo.FuelType,
                        BasePricePerDay = carInfo.Price,
                        RequiredDeposit = carInfo.Deposit,
                        ProductionYear = carInfo.Year,
                        Status = "Available",
                        Mileage = Random.Shared.Next(5000, 80000),
                        FuelConsumption = Math.Round((decimal)(Random.Shared.NextDouble() * 5 + 6), 1),
                        Description = $"{carInfo.Brand} {carInfo.Model} {carInfo.Color.ToLower()}, well-maintained vehicle suitable for family and travel.",
                        Address = $"{Random.Shared.Next(100, 999)} Street Name",
                        Province = "Ho Chi Minh City",
                        District = $"District {Random.Shared.Next(1, 13)}",
                        Ward = $"Ward {Random.Shared.Next(1, 15)}",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    context.Cars.Add(car);
                    cars.Add(car);
                    ownerIndex++;

                    logger.LogInformation("Created car: {Brand} {Model} - {LicensePlate}",
                        carInfo.Brand, carInfo.Model, carInfo.LicensePlate);
                }
                else
                {
                    cars.Add(existingCar);
                    logger.LogInformation("Car {LicensePlate} already exists", carInfo.LicensePlate);
                }
            }

            return cars;
        }

        private static async Task SeedWalletsAsync(CarRentalDbContext context, List<User> customers, ILogger<SeedData> logger)
        {
            foreach (var customer in customers)
            {
                var existingWallet = await context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == customer.Id && !w.IsDeleted);

                if (existingWallet == null)
                {
                    var wallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        UserId = customer.Id,
                        Balance = Random.Shared.Next(2000000, 15000000), // 2M - 15M VND
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    context.Wallets.Add(wallet);
                    logger.LogInformation("Created wallet for user: {Email} with balance: {Balance:N0} VND",
                        customer.Email, wallet.Balance);
                }
                else
                {
                    logger.LogInformation("Wallet for user {Email} already exists", customer.Email);
                }
            }
        }

        private static async Task SeedSampleBookingsAsync(CarRentalDbContext context, List<User> customers, List<Car> cars, ILogger<SeedData> logger)
        {
            if (!customers.Any() || !cars.Any())
            {
                logger.LogWarning("Not enough customers ({CustomerCount}) or cars ({CarCount}) to create sample bookings",
                    customers.Count, cars.Count);
                return;
            }

            // Create sample bookings
            var bookingData = new[]
            {
                new {
                    CustomerId = customers[0].Id,
                    CarId = cars[0].Id,
                    PickupDate = DateTime.Today.AddDays(-7),
                    ReturnDate = DateTime.Today.AddDays(-4),
                    Status = "Completed"
                },
                new {
                    CustomerId = customers[1].Id,
                    CarId = cars[1].Id,
                    PickupDate = DateTime.Today.AddDays(-3),
                    ReturnDate = DateTime.Today.AddDays(-1),
                    Status = "Completed"
                },
                new {
                    CustomerId = customers[2].Id,
                    CarId = cars[2].Id,
                    PickupDate = DateTime.Today.AddDays(1),
                    ReturnDate = DateTime.Today.AddDays(4),
                    Status = "Confirmed"
                },
                new {
                    CustomerId = customers[3].Id,
                    CarId = cars[3].Id,
                    PickupDate = DateTime.Today.AddDays(3),
                    ReturnDate = DateTime.Today.AddDays(6),
                    Status = "Confirmed"
                }
            };

            foreach (var bookingInfo in bookingData)
            {
                var existingBooking = await context.Bookings
                    .AnyAsync(b => b.RenterID == bookingInfo.CustomerId &&
                                  b.PickupDate == bookingInfo.PickupDate &&
                                  !b.IsDeleted);

                if (!existingBooking)
                {
                    var booking = new Booking
                    {
                        Id = Guid.NewGuid(),
                        BookingNo = GenerateBookingNumber(),
                        RenterID = bookingInfo.CustomerId,
                        PickupDate = bookingInfo.PickupDate,
                        ReturnDate = bookingInfo.ReturnDate,
                        TransactionType = "Wallet",
                        TotalAmount = CalculateTotalAmount(cars.First(c => c.Id == bookingInfo.CarId), bookingInfo.PickupDate, bookingInfo.ReturnDate),
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    context.Bookings.Add(booking);

                    // Create booking item
                    var car = cars.First(c => c.Id == bookingInfo.CarId);
                    var customer = customers.First(c => c.Id == bookingInfo.CustomerId);
                    var bookingItem = new BookingItem
                    {
                        Id = Guid.NewGuid(),
                        BookingID = booking.Id,
                        CarID = bookingInfo.CarId,
                        PricePerDay = car.BasePricePerDay,
                        Deposit = car.RequiredDeposit,
                        LicenseID = customer.LicenseId ?? $"B2-{Random.Shared.Next(100000000, 999999999)}",
                        LicenseImage = "",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    context.BookingItems.Add(bookingItem);

                    logger.LogInformation("Created sample booking: {BookingNo}", booking.BookingNo);
                }
            }
        }

        private static string GenerateBookingNumber()
        {
            return $"BK{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        }

        private static decimal CalculateTotalAmount(Car car, DateTime pickupDate, DateTime returnDate)
        {
            var days = (int)(returnDate - pickupDate).TotalDays + 1;
            return (car.BasePricePerDay ?? 0) * days;
        }

        public static void SeedBrandsFromExcel(CarRentalDbContext context)
        {
            // EPPlus License
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Abbsolute path to Excel file
            var projectRootPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            var filePath = Path.Combine(projectRootPath, "Car Rentals_Value list_Brand and model.xlsx");

            Console.WriteLine("Excel path: " + filePath);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // Get sheet count
                Console.WriteLine("Worksheets count: " + package.Workbook.Worksheets.Count);
                foreach (var ws in package.Workbook.Worksheets)
                {
                    Console.WriteLine("Found worksheet: " + ws.Name);
                }

                var worksheet = package.Workbook.Worksheets["Brand and Model"];
                if (worksheet == null)
                {
                    Console.WriteLine("Worksheet 'Brand and Model' not found.");
                    return;
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                Console.WriteLine("Rows found: " + rowCount);
                if (rowCount < 2)
                {
                    Console.WriteLine("No data to import.");
                    return;
                }

                for (int row = 2; row <= rowCount; row++) // skip header
                {
                    var brandName = worksheet.Cells[row, 2].Value?.ToString();
                    var modelName = worksheet.Cells[row, 3].Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(brandName) && !string.IsNullOrWhiteSpace(modelName))
                    {
                        bool exists = context.Brands.Any(c => c.BrandName == brandName && c.ModelName == modelName);
                        if (!exists)
                        {
                            context.Brands.Add(new Brand
                            {
                                BrandName = brandName,
                                ModelName = modelName
                            });
                            Console.WriteLine($"Added: {brandName} - {modelName}");
                        }
                        else
                        {
                            Console.WriteLine($"Skipped (exists): {brandName} - {modelName}");
                        }
                    }
                }

                context.SaveChanges();
                Console.WriteLine("Import completed.");
            }
        }
    }
}