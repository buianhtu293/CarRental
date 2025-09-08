using System.ComponentModel.DataAnnotations;

namespace CarRental.MVC.Models.Booking
{
    public class BookingInformationViewModel
    {
        // Progress information
        public BookingProgressViewModel Progress { get; set; } = new();
        
        // Car Information
        public CarInformationViewModel Car { get; set; } = new();
        
        // Booking Summary
        public BookingSummaryViewModel Summary { get; set; } = new();
        
        // Renter Information
        public RenterInformationViewModel Renter { get; set; } = new();
        
        // Driver Information (for each car)
        public List<DriverInformationViewModel> Drivers { get; set; } = new();
        
        // Session data
        public string BookingSessionId { get; set; } = string.Empty;
        public List<Guid> SelectedCarIds { get; set; } = new();
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; } = DateTime.Today.AddDays(1);
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(2);
    }
}