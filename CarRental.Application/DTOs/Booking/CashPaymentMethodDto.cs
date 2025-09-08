namespace CarRental.Application.DTOs.Booking
{
    public class CashPaymentMethodDto
    {
        public bool IsAvailable { get; set; } = true;
        public string Instructions { get; set; } = "Please pay the deposit in cash when pick-up the vehicle. Please bring your identification documents and a valid driver's license.";
        public string Note { get; set; } = "Note: The booking will be changed to the 'Pending Deposit' status until the car owner confirms the payment, after which the status will be updated to 'Confirmed'.";
    }
}