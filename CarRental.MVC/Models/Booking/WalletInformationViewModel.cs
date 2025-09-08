namespace CarRental.MVC.Models.Booking
{
    public class WalletInformationViewModel
    {
        public decimal Balance { get; set; }
        public string BalanceFormatted => Balance.ToString("N0") + " VN?";
        public bool HasWallet { get; set; } = true;
    }
}