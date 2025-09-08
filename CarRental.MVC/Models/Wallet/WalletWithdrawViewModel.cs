namespace CarRental.MVC.Models.Wallet
{
    /// <summary>
    /// Data required to render the withdraw dialog and post the selected amount.
    /// </summary>
    public class WalletWithdrawViewModel
    {
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Selected withdraw amount or decimal.MaxValue to represent "All balance".
        /// </summary>
        public decimal Amount { get; set; }

        public string? Note { get; set; }
        public bool IsAllBalance => Amount == decimal.MaxValue;
    }
}