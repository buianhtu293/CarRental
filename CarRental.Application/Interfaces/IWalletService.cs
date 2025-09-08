using CarRental.Application.DTOs.Wallet;

namespace CarRental.Application.Interfaces;

/// <summary>
/// Defines a contract for wallet-related operations
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Ensures a wallet exists for the specified user.
    /// If it does not exist, a zero-balance wallet is created.
    /// Idempotent and safe to call multiple times.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the wallet is being ensured.</param>
    Task EnsureWalletForUserAsync(Guid userId);

    /// <summary>
    /// Retrieves the wallet details for the specified user, including a paginated list of wallet entries.
    /// </summary>
    /// <param name="walletRequestDto">The request object containing user ID, date filters, and pagination information.</param>
    /// <returns>A <see cref="WalletResponseDto"/> containing the wallet balance and a paginated list of wallet entries.</returns>
    Task<WalletResponseDto> GetWalletWithEntries(WalletRequestDto walletRequestDto);

    /// <summary>
    /// Tops up the current user's wallet by <paramref name="amount"/> and writes a WalletEntry audit row.
    /// </summary>
    /// <param name="userId">Current authenticated user id.</param>
    /// <param name="amount">Amount to increase, must be &gt; 0.</param>
    /// <param name="note">Optional note saved to WalletEntry.</param>
    /// <returns>A value indicating whether the top-up operation was successful.</returns>
    Task<bool> TopUpAsync(Guid userId, decimal amount, string? note);

    /// <summary>
    /// Withdraws the specified <paramref name="amount"/> from the user's wallet if sufficient balance is available,
    /// and adds an audit entry to the wallet transaction history.
    /// </summary>
    /// <param name="userId">The unique identifier of the current authenticated user.</param>
    /// <param name="amount">The amount to withdraw, which must be greater than 0 or a special marker indicating "all".</param>
    /// <param name="note">An optional note associated with the wallet transaction.</param>
    /// <returns>A value indicating whether the withdrawal operation was successful.</returns>
    Task<bool> WithdrawAsync(Guid userId, decimal amount, string? note);
}