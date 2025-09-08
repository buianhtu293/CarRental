using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing all information required to process booking in unified booking process flow.
    /// Includes session information, selected cars, renter, drivers and financial information.
    /// </summary>
    public class BookingProcessDto
    {
        /// <summary>
        /// Session code to track booking process
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// List of selected car IDs
        /// </summary>
        public List<Guid> SelectedCarIds { get; set; } = new();
        
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
        /// Total rental amount
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Total deposit amount
        /// </summary>
        public decimal TotalDeposit { get; set; }
        
        /// <summary>
        /// Car renter information
        /// </summary>
        public RenterInformationDto Renter { get; set; } = new();
        
        /// <summary>
        /// List of driver information for each car
        /// </summary>
        public List<DriverInformationDto> Drivers { get; set; } = new();
        
        /// <summary>
        /// List of detailed information of selected cars
        /// </summary>
        public List<CarSummaryItemDto> CarItems { get; set; } = new();
    }

    /// <summary>
    /// Result returned from booking processing operation.
    /// Contains information about success/failure, booking code and validation errors.
    /// </summary>
    public class BookingProcessResult
    {
        /// <summary>
        /// Indicates whether booking process was successful
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Generated booking code (if successful)
        /// </summary>
        public string BookingNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// General error message (if failed)
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// List of detailed validation errors
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();
        
        /// <summary>
        /// Creates successful result with booking code
        /// </summary>
        /// <param name="bookingNumber">Generated booking code</param>
        /// <returns>Successful result</returns>
        public static BookingProcessResult Success(string bookingNumber)
        {
            return new BookingProcessResult
            {
                IsSuccess = true,
                BookingNumber = bookingNumber
            };
        }
        
        /// <summary>
        /// Creates failed result with error message
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <returns>Failed result</returns>
        public static BookingProcessResult Failure(string errorMessage)
        {
            return new BookingProcessResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
        
        /// <summary>
        /// Creates failed result due to validation with detailed error list
        /// </summary>
        /// <param name="validationErrors">List of validation errors</param>
        /// <returns>Validation failure result</returns>
        public static BookingProcessResult ValidationFailure(List<string> validationErrors)
        {
            return new BookingProcessResult
            {
                IsSuccess = false,
                ValidationErrors = validationErrors
            };
        }
    }
    
    /// <summary>
    /// Validation result containing list of errors found.
    /// Used to validate booking data before processing.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new();
        
        /// <summary>
        /// Indicates whether data is valid (no errors)
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds a validation error to the list
        /// </summary>
        /// <param name="field">Name of the field with error</param>
        /// <param name="message">Error message</param>
        public void AddError(string field, string message)
        {
            Errors.Add(new ValidationError { Field = field, Message = message });
        }
    }

    /// <summary>
    /// Detailed information about a validation error.
    /// Contains field name and corresponding error message.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Name of the field with error
        /// </summary>
        public string Field { get; set; } = string.Empty;
        
        /// <summary>
        /// Error description message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}