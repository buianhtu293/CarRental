namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing booking completion confirmation information including payment info, order summary and next steps.
    /// Used to display confirmation page after user successfully books a car.
    /// </summary>
    public class BookingFinishDto
    {
        /// <summary>
        /// Booking progress information (current step)
        /// </summary>
        public BookingProgressDto Progress { get; set; } = new();
        
        /// <summary>
        /// Generated booking number
        /// </summary>
        public string BookingNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Booking status
        /// </summary>
        public string BookingStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// Booking creation date
        /// </summary>
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Booking summary including cars, rental dates, pricing
        /// </summary>
        public BookingSummaryDto BookingSummary { get; set; } = new();
        
        /// <summary>
        /// Payment method used
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// Payment status
        /// </summary>
        public string PaymentStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// Title for next steps section
        /// </summary>
        public string NextStepsTitle { get; set; } = "Next Steps";
        
        /// <summary>
        /// List of next steps user needs to take
        /// </summary>
        public List<string> NextSteps { get; set; } = new();
        
        /// <summary>
        /// Support contact phone number
        /// </summary>
        public string ContactPhone { get; set; } = "0999-888-777";
        
        /// <summary>
        /// Support contact email
        /// </summary>
        public string ContactEmail { get; set; } = "support@carrental.com";
        
        /// <summary>
        /// Main success message
        /// </summary>
        public string SuccessMessage { get; set; } = "Booking Successful!";
        
        /// <summary>
        /// Detailed message about booking completion
        /// </summary>
        public string DetailMessage { get; set; } = "Thank you for using our service. Detailed booking information has been sent to your email.";
        
        /// <summary>
        /// List of action buttons that user can perform
        /// </summary>
        public Dictionary<string, string> ActionButtons { get; set; } = new()
        {
            { "GoToHomepage", "Go to Homepage" },
            { "BookAnotherCar", "Book Another Car" },
            { "ViewBooking", "View Booking Details" }
        };
    }
}