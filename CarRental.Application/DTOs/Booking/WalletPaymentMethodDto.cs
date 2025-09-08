namespace CarRental.Application.DTOs.Booking
{
    public class WalletPaymentMethodDto
    {
        public bool IsAvailable { get; set; } = true;
        public decimal CurrentBalance { get; set; }
        public decimal RequiredAmount { get; set; }
        public bool HasSufficientFunds => CurrentBalance >= RequiredAmount;
        public string BalanceStatusClass => HasSufficientFunds ? "text-success" : "text-danger";
        public string StatusText => HasSufficientFunds
            ? "Sufficient balance to proceed with the booking"
            : "Insufficient balance to proceed with the booking";
    }
}
