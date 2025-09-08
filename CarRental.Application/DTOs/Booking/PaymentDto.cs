using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing payment information in booking process.
    /// Includes information about available payment methods, digital wallet and order summary.
    /// </summary>
    public class PaymentDto
    {
        /// <summary>
        /// Current booking progress information
        /// </summary>
        public BookingProgressDto Progress { get; set; } = new();
        
        /// <summary>
        /// Booking summary to display in payment step
        /// </summary>
        public BookingSummaryDto BookingSummary { get; set; } = new();
        
        /// <summary>
        /// List of available payment methods
        /// </summary>
        public PaymentMethodsDto PaymentMethods { get; set; } = new();
        
        /// <summary>
        /// Payment method selected by user
        /// </summary>
        public PaymentMethodTypeEnum SelectedPaymentMethod { get; set; }
        
        /// <summary>
        /// Booking session code to link with booking data
        /// </summary>
        public string BookingSessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// User's digital wallet information
        /// </summary>
        public WalletInformationDto UserWallet { get; set; } = new();
    }
}