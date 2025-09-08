using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // Repository properties
        IBookingRepository Bookings { get; }
        IBookingItemRepository BookingItems { get; }
        ICarRepository Cars { get; }
        IHomepageRepository Homepages { get; }
        IWalletRepository Wallets { get; }
        ICartRepository Carts { get; }
        IFeedbackRepository Feedbacks { get; }
        ICartItemRepository CartItems { get; }
        ICarOwnerBookingRepository CarOwnerBookings { get; }

        // Transaction methods
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Generic repository access for other entities
        /// <summary>
        /// Provides a generic repository for accessing entities in the context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity which inherits from BaseEntity.</typeparam>
        /// <typeparam name="TPrimaryKey">The type of the primary key for the entity.</typeparam>
        /// <returns>A generic repository instance for the specified entity type and primary key.</returns>
        IGenericRepository<TEntity, TPrimaryKey> Repository<TEntity, TPrimaryKey>() 
            where TEntity : BaseEntity<TPrimaryKey>;

        /// <summary>
        /// Creates an additional repository instance for a specified repository type using the provided factory method and returns it along with its associated DbContext.
        /// </summary>
        /// <remarks>
        /// The DbContext created and returned from this method is the new, additional one,
        /// which the DI does not have control over it.
        /// Therefore, the developer should manually manage the life cycle of the DbContext returned by this method,
        /// e.g. by `await using` to use the auto-disposable feature of the DbContext.
        /// </remarks>
        /// <typeparam name="TRepository">The type of the repository, inheriting from IGenericRepository, to be created.</typeparam>
        /// <param name="factoryMethod">The function used to create the repository instance, taking a DbContext as input.</param>
        /// <returns>A tuple containing the created repository instance and its associated DbContext.</returns>
        public Task<(TRepository ExtraRepo, DbContext ExtraContext)> CreateExtraRepository<TRepository>(
            Func<DbContext, TRepository> factoryMethod)
            where TRepository : IGenericRepository;
    }
}
