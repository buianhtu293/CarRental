using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    public class BrandRepository : GenericRepository<Brand, Guid>, IBrandRepository
    {
        public BrandRepository(CarRentalDbContext context) : base(context)
        {
        }
    }
}