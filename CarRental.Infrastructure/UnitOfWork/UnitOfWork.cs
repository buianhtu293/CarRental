using Microsoft.EntityFrameworkCore.Storage;
using CarRental.Domain.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CarRentalDbContext _context;
        private readonly IDbContextFactory<CarRentalDbContext> _factory;
        private readonly IServiceProvider _serviceProvider;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(CarRentalDbContext context, IDbContextFactory<CarRentalDbContext> factory, IServiceProvider serviceProvider)
        {
            _context = context;
            _factory = factory;
            _serviceProvider = serviceProvider;
        }

        #region Repository Properties

        public IBookingRepository Bookings => _serviceProvider.GetRequiredService<IBookingRepository>();

        public IBookingItemRepository BookingItems => _serviceProvider.GetRequiredService<IBookingItemRepository>();

        public ICarRepository Cars => _serviceProvider.GetRequiredService<ICarRepository>();
        public IHomepageRepository Homepages => _serviceProvider.GetRequiredService<IHomepageRepository>();

        public IWalletRepository Wallets => _serviceProvider.GetRequiredService<IWalletRepository>();

        public ICartRepository Carts => _serviceProvider.GetRequiredService<ICartRepository>();

        public ICartItemRepository CartItems => _serviceProvider.GetRequiredService<ICartItemRepository>();
        public IFeedbackRepository Feedbacks => _serviceProvider.GetRequiredService<IFeedbackRepository>();

        public ICarOwnerBookingRepository CarOwnerBookings => _serviceProvider.GetRequiredService<ICarOwnerBookingRepository>();

        #endregion

        #region Transaction Methods

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion

        #region Generic Repository Access

        /// <inheritdoc />
        public IGenericRepository<TEntity, TPrimaryKey> Repository<TEntity, TPrimaryKey>()
            where TEntity : BaseEntity<TPrimaryKey>
        {
            // Resolve directly from DI container
            return _serviceProvider.GetRequiredService<IGenericRepository<TEntity, TPrimaryKey>>();
        }

        #endregion

        #region Extra: Parallel/Isolated DbContext Support

        /// <inheritdoc />
        public async Task<(TRepository ExtraRepo, DbContext ExtraContext)> CreateExtraRepository<TRepository>(
            Func<DbContext, TRepository> factoryMethod)
            where TRepository : IGenericRepository
        {
            var ctx = await _factory.CreateDbContextAsync();
            return (factoryMethod(ctx), ctx);
        }
        
        #endregion

        #region Dispose

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            await _context.DisposeAsync();
        }

        #endregion
    }
}