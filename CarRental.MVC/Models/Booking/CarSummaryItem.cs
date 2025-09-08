namespace CarRental.MVC.Models.Booking
{
    public class CarSummaryItem
    {
        public Guid CarId { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int? Seats { get; set; }
        public double AverageRating { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }
        public decimal PricePerDay { get; set; }
        public int TotalTrips { get; set; }
        public decimal Deposit { get; set; }
        public decimal SubTotal { get; set; }
        public string? ImagePath { get; set; }
    }
}