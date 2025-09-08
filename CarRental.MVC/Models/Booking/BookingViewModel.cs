using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CarRental.MVC.Models.Booking
{
    public class BookingViewModel
    {
        public BookingProgressViewModel Progress { get; set; } = new();
        
        // Step 1: Information
        public BookingInformationFormViewModel Information { get; set; } = new();
        
        // Step 2: Payment
        public BookingPaymentFormViewModel Payment { get; set; } = new();
        
        // Step 3: Confirmation
        public BookingConfirmationViewModel Confirmation { get; set; } = new();
        
        // Common data
        public string SessionId { get; set; } = string.Empty;
        public List<Guid> SelectedCarIds { get; set; } = new();
        public DateTime PickupDate { get; set; } = DateTime.Today.AddDays(1);
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(2);
        public List<CarSummaryItem> CarItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal NumberOfDays { get; set; }
        
        // Validation messages
        public List<ValidationMessage> ValidationMessages { get; set; } = new();
    }

    public class BookingInformationFormViewModel
    {
        [Required(ErrorMessage = "Please select pickup date")]
        public DateTime PickupDate { get; set; } = DateTime.Today.AddDays(1);
        
        [Required(ErrorMessage = "Please select return date")]
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(2);
        
        public RenterInformationViewModel Renter { get; set; } = new();
        public List<DriverInformationViewModel> Drivers { get; set; } = new();
    }

    public class BookingPaymentFormViewModel
    {
        [Required(ErrorMessage = "Please select a payment method")]
        public PaymentMethodTypeEnum SelectedPaymentMethod { get; set; }
        
        public WalletInformationViewModel UserWallet { get; set; } = new();
        public PaymentMethodsViewModel PaymentMethods { get; set; } = new();
    }

    public class BookingConfirmationViewModel
    {
        public string BookingNumber { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public List<string> NextSteps { get; set; } = new();
    }

    public class ValidationMessage
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ValidationLevel Level { get; set; }
        public int Step { get; set; }
    }

    public enum ValidationLevel
    {
        Error,
        Warning,
        Info
    }
}