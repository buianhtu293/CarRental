using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;

namespace CarRental.Infrastructure.Repositories
{
    public class WalletRepository : GenericRepository<Wallet, Guid>, IWalletRepository
    {
        public WalletRepository(CarRentalDbContext context) : base(context)
        {
        }

        public async Task<Wallet?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
        }

        public async Task<bool> HasSufficientBalanceAsync(Guid userId, decimal amount)
        {
            var wallet = await GetByUserIdAsync(userId);
            return wallet != null && wallet.Balance >= amount;
        }

        public async Task<bool> DeductBalanceAsync(Guid userId, decimal amount)
        {
            var wallet = await _dbSet.FromSqlRaw(@"
            SELECT * FROM Wallets WITH (UPDLOCK, ROWLOCK) 
            WHERE UserId = {0}", userId).FirstOrDefaultAsync();

            if (wallet == null || wallet.Balance < amount)
            {
                return false;
            }

            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(wallet);

            return true;
        }

        public async Task<bool> AddBalanceAsync(Guid userId, decimal amount)
        {
            var wallet = await _dbSet.FromSqlRaw(@"
            SELECT * FROM Wallets WITH (UPDLOCK, ROWLOCK) 
            WHERE UserId = {0}", userId).FirstOrDefaultAsync();

            if (wallet == null)
            {
                return false;
            }

            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(wallet);

            return true;
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var wallet = await GetByUserIdAsync(userId);
            return wallet?.Balance ?? 0;
        }

        public async Task<Wallet> CreateWalletForUserAsync(Guid userId)
        {
            // In this method, No need to check if a wallet exists,
            // as we have 'unique index constraint' on wallet and userId in the `CarRentalDbContext`.
            // If a duplicate is attempted (for example, someone calls this method twice then calls SaveChanges),
            // it will throw an exception.
            // To prevent exception,
            // we can check if it exists first in the Application layer, or anywhere we call this method.
            
            // The reason for this is to ensure that we don't create multiple wallets for the same user.

            var newWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            return await AddAsync(newWallet);
        }

        public async Task<bool> ImplementTransaction(Guid renterId, Guid ownerId, decimal amount)
        {
            var lstWallet = await _dbSet.FromSqlRaw(@"
            SELECT * FROM Wallets WITH (UPDLOCK, ROWLOCK) 
            WHERE Id IN ({0}, {1})", renterId, ownerId).ToListAsync();

            var renterWallet = lstWallet.First(x => x.UserId == renterId);
            var ownerWallet = lstWallet.First(x => x.UserId == ownerId);

            if (renterWallet.Balance < amount)
            {
                throw new Exception("Money don't have enough to pay;");
                return false;
            }

            renterWallet.Balance -= amount;
            ownerWallet.Balance += amount;

            return true;
        }

        # region Wallet entry

        public async Task<Wallet?> GetWalletWithEntriesAndCarName(Guid userId, DateTime pickupDate, DateTime returnDate, Func<IQueryable<Wallet>, IQueryable<Wallet>>? options = null)
        {
            var query = _context.Wallets
                .Include(w => w.Entries.Where(entry => entry.CreatedAt >= pickupDate && entry.CreatedAt <= returnDate && !entry.IsDeleted).OrderByDescending(entry => entry.CreatedAt))
                .ThenInclude(w => w.Booking!.BookingItems.Where(bi => !bi.IsDeleted))
                .ThenInclude(bi => bi.Car)
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .OrderBy(w => w.CreatedAt)
                .AsQueryable();

            query = ApplyQueryOptions(query, options);

            return await query.FirstOrDefaultAsync();
        }

        #endregion
    }
}