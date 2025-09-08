namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing all information required for car booking process including car info, renter, drivers and order summary.
    /// Used in the information input step of the booking process.
    /// </summary>
    public class BookingInformationDto
    {
        /// <summary>
        /// Current booking progress information
        /// </summary>
        public BookingProgressDto Progress { get; set; } = new();
        
        /// <summary>
        /// Booking summary including selected cars, pricing, rental time
        /// </summary>
        public BookingSummaryDto Summary { get; set; } = new();
        
        /// <summary>
        /// Car renter information
        /// </summary>
        public RenterInformationDto Renter { get; set; } = new();
        
        /// <summary>
        /// List of driver information for each car (each car must have one driver)
        /// </summary>
        public List<DriverInformationDto> Drivers { get; set; } = new();
        
        /// <summary>
        /// Session code to track booking process
        /// </summary>
        public string BookingSessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// List of selected car IDs
        /// </summary>
        public List<Guid> SelectedCarIds { get; set; } = new();
        
        /// <summary>
        /// Car pickup date (default is tomorrow)
        /// </summary>
        public DateTime PickupDate { get; set; } = DateTime.Today.AddDays(1);
        
        /// <summary>
        /// Car return date (default is day after tomorrow)
        /// </summary>
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(2);
    }
}