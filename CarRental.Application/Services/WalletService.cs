using CarRental.Application.DTOs.Wallet;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarRental.Application.Services;

/// <inheritdoc />
public class WalletService : IWalletService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IUnitOfWork unitOfWork, ILogger<WalletService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task EnsureWalletForUserAsync(Guid userId)
    {
        // NOTE: We first check, then create if missing.
        var existing = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
        if (existing != null) return;

        try
        {
            await _unitOfWork.Wallets.CreateWalletForUserAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Wallet provisioned for user {UserId}", userId);
        }
        catch (DbUpdateException ex)
        {
            // Unique index on Wallet.UserId means a concurrent creation could happen.
            // If that happens, we swallow and process the exception; any other DB error should be logged.
            _logger.LogWarning(ex, "Race creating wallet for user {UserId}. Fetching existing wallet", userId);

            // Fetch the wallet created by the other thread
            var createdByOtherThread = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (createdByOtherThread == null)
            {
                throw; // Unexpected case — rethrow
            }
        }
    }

    /// <inheritdoc />
    public async Task<WalletResponseDto> GetWalletWithEntries(WalletRequestDto walletRequestDto)
    {
        if (walletRequestDto == null)
        {
            throw new ArgumentNullException(nameof(walletRequestDto));
        }

        // walletRequestDto.UserId = Guid.Parse("b44b363d-27ca-43c1-8441-ccaac09cd4ed");

        // default To = now, From = 1 month ago if missing (BRL-24-05)
        var toDate = walletRequestDto.ReturnDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow;
        var fromDate = walletRequestDto.PickupDate?.Date ?? toDate.AddMonths(-1).Date;

        if (toDate < fromDate)
        {
            throw new ArgumentException("The end date must be later than the start date in order to search for transactions. Please try again.");
        }

        walletRequestDto.Page = Math.Max(1, walletRequestDto.Page);
        walletRequestDto.PageSize = Math.Clamp(walletRequestDto.PageSize, 1, 100);

        var walletWithEntries = await _unitOfWork.Wallets.GetWalletWithEntriesAndCarName(walletRequestDto.UserId, fromDate, toDate, w => w.AsNoTracking());
        if (walletWithEntries == null)
        {
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(walletRequestDto.UserId);
            if (wallet == null)
            {
                throw new InvalidOperationException($"Wallet for user {walletRequestDto.UserId} not found.");
            }

            return new WalletResponseDto([], 0, 0, walletRequestDto.PageSize)
            {
                Balance = wallet.Balance
            };
        }

        var entries = walletWithEntries.Entries;
        var totalCount = entries.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)walletRequestDto.PageSize);
        var pageNumber = Math.Min(walletRequestDto.Page, totalPages);
        var skip = (pageNumber - 1) * walletRequestDto.PageSize;
        var take = walletRequestDto.PageSize;

        var walletEntries = entries.Skip(skip).Take(take).Select(e => new WalletEntryResponseDto()
        {
            BookingNumber = e.Booking?.BookingNo,
            Amount = e.Amount,
            CreatedAt = e.CreatedAt,
            Type = e.Type,
            CarName = e.Booking?.BookingItems.Select(bi => bi.Car.Model).FirstOrDefault(),
            Note = e.Note,
        }).ToList();

        var ret = new WalletResponseDto(walletEntries, totalCount, pageNumber, walletRequestDto.PageSize)
        {
            Balance = walletWithEntries.Balance
        };

        return ret;
    }

    /// <inheritdoc />
    public async Task<bool> TopUpAsync(Guid userId, decimal amount, string? note)
    {
        // userId = Guid.Parse("b44b363d-27ca-43c1-8441-ccaac09cd4ed");

        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("User is not identified.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId)
                     ?? throw new InvalidOperationException($"Wallet for user {userId} not found.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isSuccessful = await _unitOfWork.Wallets.AddBalanceAsync(wallet.UserId, amount);
            if (!isSuccessful)
            {
                return false;
            }

            // audit entry
            var entryRepo = _unitOfWork.Repository<WalletEntry, Guid>();
            await entryRepo.AddAsync(new WalletEntry
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Type = WalletEntryType.TopUp,
                Note = string.IsNullOrWhiteSpace(note) ? "Top-up" : note,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Top-up failed");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> WithdrawAsync(Guid userId, decimal amount, string? note)
    {
        // userId = Guid.Parse("b44b363d-27ca-43c1-8441-ccaac09cd4ed");

        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("User is not identified.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId)
                     ?? throw new InvalidOperationException($"Wallet for user {userId} not found.");

        // If caller passed decimal.MaxValue as the "ALL" marker we use the current balance.
        var withdrawAmount = amount == decimal.MaxValue ? wallet.Balance : amount;

        if (withdrawAmount <= 0)
        {
            throw new InvalidOperationException("Nothing to withdraw.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isSuccessful = await _unitOfWork.Wallets.DeductBalanceAsync(wallet.UserId, withdrawAmount);
            if (!isSuccessful)
            {
                return false;
            }

            var entryRepo = _unitOfWork.Repository<WalletEntry, Guid>();
            await entryRepo.AddAsync(new WalletEntry
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = -withdrawAmount,
                Type = WalletEntryType.Withdraw,
                Note = string.IsNullOrWhiteSpace(note) ? "Withdraw" : note,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Withdraw failed");
            return false;
        }
    }
}