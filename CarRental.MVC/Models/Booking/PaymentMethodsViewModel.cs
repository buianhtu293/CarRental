namespace CarRental.MVC.Models.Booking
{
    public class PaymentMethodsViewModel
    {
        public WalletPaymentMethodViewModel Wallet { get; set; } = new();
        public CashPaymentMethodViewModel Cash { get; set; } = new();
        public BankTransferPaymentMethodViewModel BankTransfer { get; set; } = new();
    }
}