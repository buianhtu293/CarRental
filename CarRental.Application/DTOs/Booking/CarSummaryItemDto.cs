namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing summary information of a car in booking.
    /// Displays basic information, pricing and calculations for each car in the booking order.
    /// </summary>
    public class CarSummaryItemDto
    {
        /// <summary>
        /// Unique identifier of the car
        /// </summary>
        public Guid CarId { get; set; }
        
        /// <summary>
        /// Car name (brand + model)
        /// </summary>
        public string CarName { get; set; } = string.Empty;
        
        /// <summary>
        /// Car license plate number
        /// </summary>
        public string LicensePlate { get; set; } = string.Empty;
        
        /// <summary>
        /// Car location
        /// </summary>
        public string Location { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of seats in the car
        /// </summary>
        public int? Seats { get; set; }
        
        /// <summary>
        /// Average rating of the car (from 0-5 stars)
        /// </summary>
        public double AverageRating { get; set; }
        
        /// <summary>
        /// Transmission type (Manual, Automatic)
        /// </summary>
        public string? Transmission { get; set; }
        
        /// <summary>
        /// Fuel type (Gasoline, Diesel, Electric, etc.)
        /// </summary>
        public string? FuelType { get; set; }
        
        /// <summary>
        /// Daily rental price (VND)
        /// </summary>
        public decimal PricePerDay { get; set; }
        
        /// <summary>
        /// Total number of completed trips
        /// </summary>
        public int TotalTrips { get; set; }
        
        /// <summary>
        /// Required deposit amount (VND)
        /// </summary>
        public decimal Deposit { get; set; }
        
        /// <summary>
        /// Total amount for this car (PricePerDay × number of days)
        /// </summary>
        public decimal SubTotal { get; set; }
    }
}