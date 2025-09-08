namespace CarRental.MVC.Models.Booking
{
    /// <summary>
    /// ViewModel for booking edit page, contains booking information and allows user to update information.
    /// Used in BookingList Edit page to display and edit booking information.
    /// </summary>
    public class BookingEditViewModel
    {
        /// <summary>
        /// Booking item ID
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Main booking ID
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
        /// Current booking status
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether booking can be edited
        /// </summary>
        public bool IsEditable { get; set; }
        
        /// <summary>
        /// Payment method used
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed information about car in booking
        /// </summary>
        public CarInformationViewModel CarInfo { get; set; } = new();
        
        /// <summary>
        /// Editable booking information (rental dates, renter, drivers)
        /// </summary>
        public BookingInformationEditViewModel Information { get; set; } = new();
        
        /// <summary>
        /// Editing progress information (current step)
        /// </summary>
        public BookingProgressViewModel Progress { get; set; } = new();
        
        /// <summary>
        /// Total car rental amount
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Total deposit amount
        /// </summary>
        public decimal TotalDeposit { get; set; }
        
        /// <summary>
        /// Number of rental days
        /// </summary>
        public decimal NumberOfDays { get; set; }
    }

    /// <summary>
    /// ViewModel containing editable booking information.
    /// Includes rental dates and renter, driver information.
    /// </summary>
    public class BookingInformationEditViewModel
    {
        /// <summary>
        /// Car pickup date (editable)
        /// </summary>
        public DateTime PickupDate { get; set; }
        
        /// <summary>
        /// Car return date (editable)
        /// </summary>
        public DateTime ReturnDate { get; set; }
        
        /// <summary>
        /// Car renter information (editable)
        /// </summary>
        public RenterInformationViewModel Renter { get; set; } = new();
        
        /// <summary>
        /// Driver information list (editable)
        /// </summary>
        public List<DriverInformationViewModel> Drivers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for car return modal, displays financial information and processing options.
    /// Used in car return confirmation popup with refund calculation or additional charges.
    /// </summary>
    public class ReturnCarModalViewModel
    {
        /// <summary>
        /// Booking item ID to return car
        /// </summary>
        public Guid BookingItemId { get; set; }
        
        /// <summary>
        /// Name of car being returned
        /// </summary>
        public string CarName { get; set; } = string.Empty;
        
        /// <summary>
        /// License plate of car being returned
        /// </summary>
        public string LicensePlate { get; set; } = string.Empty;
        
        /// <summary>
        /// Car rental amount
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
        /// Amount to be refunded (if any)
        /// </summary>
        public decimal RefundAmount { get; set; }
        
        /// <summary>
        /// Additional charge amount required (if any)
        /// </summary>
        public decimal AdditionalChargeNeeded { get; set; }
        
        /// <summary>
        /// Indicates whether car return can be processed (sufficient funds in wallet)
        /// </summary>
        public bool CanProcessReturn { get; set; }
        
        /// <summary>
        /// Indicates whether wallet processing is required
        /// </summary>
        public bool RequiresWalletProcessing { get; set; }
        
        /// <summary>
        /// Processing scenario type: "insufficient" (insufficient funds), "refund" (refund), "exact" (exact match), "additional" (need to pay more)
        /// </summary>
        public string ScenarioType { get; set; } = string.Empty;
    }
}