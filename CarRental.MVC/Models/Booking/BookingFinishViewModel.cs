using System.ComponentModel.DataAnnotations;

namespace CarRental.MVC.Models.Booking
{
    public class BookingFinishViewModel
    {
        // Progress information
        public BookingProgressViewModel Progress { get; set; } = new();
        
        // Booking confirmation details
        public string BookingNumber { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        // Summary information
        public BookingSummaryViewModel BookingSummary { get; set; } = new();
        
        // Payment information
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        
        // Next steps information
        public string NextStepsTitle { get; set; } = "Next Steps";
        public List<string> NextSteps { get; set; } = new();
        
        // Contact information
        public string ContactPhone { get; set; } = "0999-888-777";
        public string ContactEmail { get; set; } = "support@carrental.com";
        
        // Success message
        public string SuccessMessage { get; set; } = "Booking Successful!";
        public string DetailMessage { get; set; } = "Thank you for using our service. Detailed booking information has been sent to your email.";
        
        // Action buttons
        public Dictionary<string, string> ActionButtons { get; set; } = new()
        {
            { "GoToHomepage", "Go to Homepage" },
            { "BookAnotherCar", "Book Another Car" },
            { "ViewBooking", "View Booking Details" }
        };
        
        public string GetPaymentStatusClass()
        {
            return PaymentStatus.ToLower() switch
            {
                "paid" => "badge-success",
                "pending" => "badge-warning",
                "failed" => "badge-danger",
                _ => "badge-secondary"
            };
        }
        
        public string GetPaymentStatusText()
        {
            return PaymentStatus.ToLower() switch
            {
                "paid" => "Paid",
                "pending" => "Pending",
                "failed" => "Failed",
                _ => "Unknown"
            };
        }
        
        public string GetBookingStatusClass()
        {
            return BookingStatus.ToLower() switch
            {
                "confirmed" => "badge-success",
                "pending" => "badge-warning",
                "cancelled" => "badge-danger",
                _ => "badge-secondary"
            };
        }
        
        public string GetBookingStatusText()
        {
            return BookingStatus.ToLower() switch
            {
                "confirmed" => "Confirmed",
                "pending" => "Pending",
                "cancelled" => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}