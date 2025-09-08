using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces
{
    public interface IWalletRepository : IGenericRepository<Wallet, Guid>
    {
        Task<Wallet?> GetByUserIdAsync(Guid userId);
        Task<bool> HasSufficientBalanceAsync(Guid userId, decimal amount);
        Task<bool> DeductBalanceAsync(Guid userId, decimal amount);
        Task<bool> AddBalanceAsync(Guid userId, decimal amount);
        Task<decimal> GetBalanceAsync(Guid userId);
        Task<Wallet> CreateWalletForUserAsync(Guid userId);
        Task<bool> ImplementTransaction(Guid rentalId, Guid ownerId, decimal amount);

        # region Wallet with WalletEntry

        Task<Wallet?> GetWalletWithEntriesAndCarName(Guid userId, DateTime pickupDate, DateTime returnDate, Func<IQueryable<Wallet>, IQueryable<Wallet>>? options = null);

        #endregion
    }
}