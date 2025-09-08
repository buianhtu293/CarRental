using CarRental.Application.DTOs.Wallet;
using ShopNow.Presentation.Models; // Using existing PagingModel from the project

namespace CarRental.MVC.Models.Wallet
{
    /// <summary>
    /// View model for the Wallet Index page (UC24: View Wallet).
    /// Encapsulates current balance, filter options, transaction list, and pagination data.
    /// </summary>
    public sealed class WalletIndexViewModel
    {
        /// <summary>
        /// Current wallet balance of the logged-in user (in VND).
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// List of wallet transactions (already paginated) returned from the service.
        /// </summary>
        public IReadOnlyList<WalletEntryResponseDto> Transactions { get; set; } = Array.Empty<WalletEntryResponseDto>();

        /// <summary>
        /// Pagination data used by the existing _Paging.cshtml partial view.
        /// </summary>
        public PagingModel Paging { get; set; } = new PagingModel();

        /// <summary>
        /// Optional filter: starting date for the transaction list.
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Optional filter: ending date for the transaction list.
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}