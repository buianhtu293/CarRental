namespace CarRental.Application.DTOs
{
    public class CarDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public int Seats { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateCarDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }

        // Step 1
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int ProductionYear { get; set; }
        public string Color { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public int Seats { get; set; }

        // Step 2
        public int Mileage { get; set; }
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Step 3
        public decimal BasePricePerDay { get; set; }
        public decimal RequiredDeposit { get; set; }
        public string? Description { get; set; }

        // Common
        public string Status { get; set; } = "Available";

        // Documents & Images
        public List<CarDocumentDto> CarDocuments { get; set; } = new();

        // Optional specifications
        public List<CarSpecificationDto> CarSpecifications { get; set; } = new();
    }

    public class CarSearchDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public decimal? PricePerDay { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public int? Seats { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Address { get; set; }
        public decimal? BasePricePerDay { get; set; }
        public int NumberOfRides { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool HasRatings => TotalReviews > 0;
        public string FormattedAverageRating => AverageRating?.ToString("F1") ?? "0.0";
        public int FullStars => AverageRating.HasValue ? (int)Math.Floor(AverageRating.Value) : 0;
        public bool HasHalfStar => AverageRating.HasValue && (AverageRating.Value - Math.Floor(AverageRating.Value)) >= 0.5;
        public int EmptyStars => 5 - FullStars - (HasHalfStar ? 1 : 0);
    }

    public class ListMyCarDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public decimal? PricePerDay { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public int? Seats { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Address { get; set; }
        public decimal? BasePricePerDay { get; set; }
        public int NumberOfRides { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool HasRatings => TotalReviews > 0;
        public string FormattedAverageRating => AverageRating?.ToString("F1") ?? "0.0";
        public int FullStars => AverageRating.HasValue ? (int)Math.Floor(AverageRating.Value) : 0;
        public bool HasHalfStar => AverageRating.HasValue && (AverageRating.Value - Math.Floor(AverageRating.Value)) >= 0.5;
        public int EmptyStars => 5 - FullStars - (HasHalfStar ? 1 : 0);
        public List<CarDocumentDto> CarDocuments { get; set; } = new();
    }

    public class EditCarDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? Mileage { get; set; }
        public decimal? FuelConsumption { get; set; }
        public int? Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public decimal? PricePerDay { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public int? Seats { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Address { get; set; }
        public decimal? BasePricePerDay { get; set; }
        public decimal? RequiredDeposit { get; set; }
        public int NumberOfRides { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool HasRatings => TotalReviews > 0;
        public string FormattedAverageRating => AverageRating?.ToString("F1") ?? "0.0";
        public int FullStars => AverageRating.HasValue ? (int)Math.Floor(AverageRating.Value) : 0;
        public bool HasHalfStar => AverageRating.HasValue && (AverageRating.Value - Math.Floor(AverageRating.Value)) >= 0.5;
        public int EmptyStars => 5 - FullStars - (HasHalfStar ? 1 : 0);
        public List<CarDocumentDto> CarDocuments { get; set; } = new();
        public List<CarSpecificationDto> CarSpecifications { get; set; } = new();
    }
}