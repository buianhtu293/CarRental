using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing detailed information of a booking item including car, renter, driver and payment information.
    /// Used to display and edit booking information in the user interface.
    /// </summary>
    public class BookingDetailDto
    {
        /// <summary>
        /// Unique identifier of the booking item
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Identifier of the main booking
        /// </summary>
        public Guid BookingId { get; set; }
        
        /// <summary>
        /// Booking number displayed to user
        /// </summary>
        public string BookingNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Booking creation date
        /// </summary>
        public DateTime BookingDate { get; set; }
        
        /// <summary>
        /// Current status of the booking item
        /// </summary>
        public BookingItemStatusEnum Status { get; set; }
        
        /// <summary>
        /// Car pickup date
        /// </summary>
        public DateTime PickupDate { get; set; }
        
        /// <summary>
        /// Car return date
        /// </summary>
        public DateTime ReturnDate { get; set; }
        
        /// <summary>
        /// Number of rental days
        /// </summary>
        public decimal NumberOfDays { get; set; }
        
        /// <summary>
        /// Payment method used
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether the booking can be edited
        /// </summary>
        public bool IsEditable { get; set; }
        
        /// <summary>
        /// Detailed information about the rented car
        /// </summary>
        public CarInformationDto CarInfo { get; set; } = new();
        
        /// <summary>
        /// Information about the car renter
        /// </summary>
        public RenterInformationDto RenterInfo { get; set; } = new();
        
        /// <summary>
        /// List of drivers authorized to drive the car
        /// </summary>
        public List<DriverInformationDto> Drivers { get; set; } = new();
        
        /// <summary>
        /// Total rental amount
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Total deposit amount
        /// </summary>
        public decimal TotalDeposit { get; set; }
    }

    /// <summary>
    /// DTO containing information to be updated for booking detail.
    /// Used when user edits booking information.
    /// </summary>
    public class BookingDetailUpdateDto
    {
        /// <summary>
        /// New pickup date
        /// </summary>
        public DateTime PickupDate { get; set; }
        
        /// <summary>
        /// New return date
        /// </summary>
        public DateTime ReturnDate { get; set; }
        
        /// <summary>
        /// Updated renter information
        /// </summary>
        public RenterInformationDto RenterInfo { get; set; } = new();
        
        /// <summary>
        /// Updated list of drivers
        /// </summary>
        public List<DriverInformationDto> Drivers { get; set; } = new();
    }

    /// <summary>
    /// DTO containing car return processing result including financial information and required actions.
    /// Used to display refund information or additional charges when returning a car.
    /// </summary>
    public class ReturnCarResultDto
    {
        /// <summary>
        /// Indicates whether car return processing was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Processing result message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether wallet processing is required
        /// </summary>
        public bool RequiresWalletProcessing { get; set; }
        
        /// <summary>
        /// Rental amount
        /// </summary>
        public decimal RentalAmount { get; set; }
        
        /// <summary>
        /// Deposit amount
        /// </summary>
        public decimal DepositAmount { get; set; }
        
        /// <summary>
        /// User's current wallet balance
        /// </summary>
        public decimal CurrentWalletBalance { get; set; }
        
        /// <summary>
        /// Amount to be refunded
        /// </summary>
        public decimal RefundAmount { get; set; }
        
        /// <summary>
        /// Additional charge amount required
        /// </summary>
        public decimal AdditionalChargeNeeded { get; set; }
        
        /// <summary>
        /// Indicates whether car return can be processed (sufficient funds in wallet)
        /// </summary>
        public bool CanProcessReturn { get; set; }
    }
}