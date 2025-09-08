using CarRental.Application.DTOs.Wallet;
using CarRental.Application.Interfaces;
using CarRental.MVC.Extensions;
using CarRental.MVC.Models.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNow.Presentation.Models;

namespace CarRental.MVC.Controllers
{
    /// <summary>
    /// MVC controller for Wallet feature (UC24: View Wallet).
    /// Responsible for rendering the wallet page and handling AJAX requests
    /// for filtering and paginating wallet transactions.
    /// </summary>
    [Authorize]
    public sealed class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Displays the main Wallet page with current balance, filters, and transaction list.
        /// </summary>
        /// <param name="fromDate">Optional start date filter.</param>
        /// <param name="toDate">Optional end date filter.</param>
        /// <param name="page">Current page number (default is 1).</param>
        /// <param name="pageSize">Number of transactions per page (default is 10).</param>
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();

            var request = new WalletRequestDto
            {
                UserId = userId,
                PickupDate = fromDate,
                ReturnDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var wallet = await _walletService.GetWalletWithEntries(request);

            var vm = wallet.ToViewModel();
            vm.FromDate = fromDate;
            vm.ToDate = toDate;
            vm.Paging = new PagingModel
            {
                currentpage = page,
                countpages = wallet.TotalPages,
                generateUrl = p => Url.Action(nameof(TransactionsPartial), new { fromDate, toDate, page = p ?? 1, pageSize })!
            };

            return View(vm);
        }

        /// <summary>
        /// Loads only the transaction table portion for AJAX filter and pagination updates.
        /// </summary>
        /// <param name="fromDate">Optional start date filter.</param>
        /// <param name="toDate">Optional end date filter.</param>
        /// <param name="page">Current page number (default is 1).</param>
        /// <param name="pageSize">Number of transactions per page (default is 10).</param>
        [HttpGet]
        public async Task<IActionResult> TransactionsPartial(DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();

            var request = new WalletRequestDto
            {
                UserId = userId,
                PickupDate = fromDate,
                ReturnDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var wallet = await _walletService.GetWalletWithEntries(request);

            var paging = new PagingModel
            {
                currentpage = page,
                countpages = wallet.TotalPages,
                generateUrl = p => Url.Action(nameof(TransactionsPartial), new { fromDate, toDate, page = p ?? 1, pageSize })!
            };

            ViewBag.Paging = paging;

            // Ensure only the partial view is returned without layout
            return PartialView("_WalletTransactions", wallet.Items);
        }

        /// <summary>
        /// Retrieves the currently logged-in user's ID from authentication claims.
        /// Assumes that ClaimTypes.NameIdentifier (or equivalent) contains a Guid value.
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var sub = User.Claims.FirstOrDefault(c =>
                c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;

            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }

        #region Top-up (Alternative flow 1)

        private Guid CurrentUserId =>
            // Solution already resolved user id; keep the same source (claims/session).
            // Fallback to Guid.Empty will be validated by the service.
            // Guid.TryParse(User?.FindFirst("sub")?.Value ?? string.Empty, out var id) ? id : Guid.Empty;
            GetCurrentUserId();

        /// <summary>
        /// Returns the top-up modal content.
        /// </summary>
        [HttpGet]
        public IActionResult TopUpDialog(decimal currentBalance)
        {
            var vm = new WalletTopUpViewModel { CurrentBalance = currentBalance };
            return PartialView("~/Views/Wallet/_TopUpModal.cshtml", vm);
        }

        /// <summary>
        /// Executes the top-up with the selected amount via AJAX.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUp([FromForm] WalletTopUpViewModel model)
        {
            var isSuccessful = await _walletService.TopUpAsync(CurrentUserId, model.Amount, model.Note);
            return isSuccessful ? Json(new { ok = true, message = "Top-up successful." }) : BadRequest(Json(new { ok = false, message = "Top-up failed." }));
        }

        #endregion

        #region Withdraw (Alternative flow 2)

        /// <summary>
        /// Returns the withdraw modal content.
        /// </summary>
        [HttpGet]
        public IActionResult WithdrawDialog(decimal currentBalance)
        {
            var vm = new WalletWithdrawViewModel { CurrentBalance = currentBalance };
            return PartialView("~/Views/Wallet/_WithdrawModal.cshtml", vm);
        }

        /// <summary>
        /// Executes the withdraw with the selected amount via AJAX.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw([FromForm] WalletWithdrawViewModel model)
        {
            // Map “All balance” to the agreed service marker.
            var amount = model.Amount == decimal.MaxValue ? decimal.MaxValue : model.Amount;
            var isSuccessful = await _walletService.WithdrawAsync(CurrentUserId, amount, model.Note);

            return isSuccessful ? Json(new { ok = true, message = "Withdraw successful." }) : BadRequest(Json(new { ok = false, message = "Withdraw failed." }));
        }

        #endregion
    }
}