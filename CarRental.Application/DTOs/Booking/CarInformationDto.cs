namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing detailed car information used in the booking process.
    /// Includes basic car information, pricing, ratings, and technical specifications.
    /// </summary>
    public class CarInformationDto
    {
        /// <summary>
        /// Unique identifier of the car
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Car brand (Toyota, Honda, etc.)
        /// </summary>
        public string Brand { get; set; } = string.Empty;
        
        /// <summary>
        /// Car model (Camry, Civic, etc.)
        /// </summary>
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// Car license plate number
        /// </summary>
        public string LicensePlate { get; set; } = string.Empty;
        
        /// <summary>
        /// Primary image URL of the car
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Daily rental price (VND)
        /// </summary>
        public decimal PricePerDay { get; set; }
        
        /// <summary>
        /// Required deposit amount (VND)
        /// </summary>
        public decimal RequiredDeposit { get; set; }
        
        /// <summary>
        /// Average rating of the car (from 0-5 stars)
        /// </summary>
        public double AverageRating { get; set; }
        
        /// <summary>
        /// Total number of completed trips
        /// </summary>
        public int TotalTrips { get; set; }
        
        /// <summary>
        /// Car location (address)
        /// </summary>
        public string Location { get; set; } = string.Empty;
        
        /// <summary>
        /// Current status of the car (Available, Rented, Maintenance, etc.)
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Car color
        /// </summary>
        public string Color { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of seats in the car
        /// </summary>
        public int? Seats { get; set; }
        
        /// <summary>
        /// Transmission type (Manual, Automatic)
        /// </summary>
        public string? Transmission { get; set; }
        
        /// <summary>
        /// Fuel type (Gasoline, Diesel, Electric, etc.)
        /// </summary>
        public string? FuelType { get; set; }
    }
}