namespace CarRental.Application.DTOs.Booking
{
    public class PaymentMethodsDto
    {
        public WalletPaymentMethodDto Wallet { get; set; } = new();
        public CashPaymentMethodDto Cash { get; set; } = new();
        public BankTransferPaymentMethodDto BankTransfer { get; set; } = new();
    }
}