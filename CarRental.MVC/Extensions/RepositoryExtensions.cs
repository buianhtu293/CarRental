using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Repositories;
using CarRental.Infrastructure.UnitOfWork;

namespace CarRental.MVC.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register specific repositories
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingItemRepository, BookingItemRepository>();
            services.AddScoped<ICarRepository, CarRepository>();
            services.AddScoped<IHomepageRepository, HomepageRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<ICarOwnerBookingRepository, CarOwnerBookingRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();

            // Register generic repository
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}