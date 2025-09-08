using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CarRental.Infrastructure.Data;
using CarRental.Application.Interfaces;
using CarRental.MVC.Services;
using CarRental.Domain.Entities;

namespace CarRental.MVC.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Database Context
            services.AddDbContext<CarRentalDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            // Extra DbContextFactory for scenarios where DbContext needs to be created on demand
            // This is useful for background services or other scenarios where dependency injection is not available
            services.AddDbContextFactory<CarRentalDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

            // Add Token Service
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}