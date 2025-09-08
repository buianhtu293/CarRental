using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CarRental.MVC.Models.Booking
{
    public class PaymentViewModel
    {
        // Progress information
        public BookingProgressViewModel Progress { get; set; } = new();
        
        // Booking summary
        public BookingSummaryViewModel BookingSummary { get; set; } = new();
        
        // Payment methods
        public PaymentMethodsViewModel PaymentMethods { get; set; } = new();
        
        // Selected payment method
        [Required(ErrorMessage = "Please select a payment method")]
        public PaymentMethodTypeEnum SelectedPaymentMethod { get; set; }
        
        // Session data
        public string BookingSessionId { get; set; } = string.Empty;
        
        // User wallet information
        public WalletInformationViewModel UserWallet { get; set; } = new();
    }
}