namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing user's digital wallet information.
    /// Displays current balance and wallet status in booking process.
    /// </summary>
    public class WalletInformationDto
    {
        /// <summary>
        /// Current balance in wallet (VND)
        /// </summary>
        public decimal Balance { get; set; }
        
        /// <summary>
        /// Formatted balance for display (with comma separator and VND unit)
        /// </summary>
        public string BalanceFormatted => Balance.ToString("N0") + " VNĐ";
        
        /// <summary>
        /// Indicates whether user has a digital wallet
        /// </summary>
        public bool HasWallet { get; set; } = true;
    }
}