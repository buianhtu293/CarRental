namespace CarRental.MVC.Models.Wallet
{
    /// <summary>
    /// Data required to render the top-up dialog and post the selected amount.
    /// </summary>
    public class WalletTopUpViewModel
    {
        public decimal CurrentBalance { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}