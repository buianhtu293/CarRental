namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing information about bank transfer payment method.
    /// Provides instructions and notes for users when using this payment method.
    /// </summary>
    public class BankTransferPaymentMethodDto
    {
        /// <summary>
        /// Indicates whether bank transfer method is available
        /// </summary>
        public bool IsAvailable { get; set; } = true;
        
        /// <summary>
        /// Instructions for using bank transfer method
        /// </summary>
        public string Instructions { get; set; } = "Please pay the deposit in bank transfer when pick-up the vehicle. Please bring your identification documents and a valid driver's license.";
        
        /// <summary>
        /// Note about booking processing workflow when using bank transfer
        /// </summary>
        public string Note { get; set; } = "Note: The booking will be changed to the 'Pending Deposit' status until the car owner confirms the payment, after which the status will be updated to 'Confirmed'.";
    }
}