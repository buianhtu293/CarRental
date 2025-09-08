namespace CarRental.MVC.Models.Booking
{
    public class WalletPaymentMethodViewModel
    {
        public bool IsAvailable { get; set; } = true;
        public decimal CurrentBalance { get; set; }
        public decimal RequiredAmount { get; set; }
        public bool HasSufficientFunds => CurrentBalance >= RequiredAmount;
        public string BalanceStatusClass => HasSufficientFunds ? "text-success" : "text-danger";
        public string StatusText => HasSufficientFunds ? "?? s? d?" : "Không ?? s? d?";
    }
}