namespace CarRental.MVC.Models.Booking
{
    public class CarInformationViewModel
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Name => $"{Brand} {Model}";
        public string LicensePlate { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal RequiredDeposit { get; set; }
        public double AverageRating { get; set; }
        public int TotalTrips { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int? Seats { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }
    }
}