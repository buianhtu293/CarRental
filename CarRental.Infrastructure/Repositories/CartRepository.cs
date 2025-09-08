using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CarRental.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<Cart, Guid>, ICartRepository
    {
        public CartRepository(CarRentalDbContext context) : base(context)
        {
        }

        public Cart GetFirstOrDefaultWithThenIncluding(
    Expression<Func<Cart, bool>> predicate,
    Func<IQueryable<Cart>, IIncludableQueryable<Cart, object>> include = null)
        {
            IQueryable<Cart> query = _dbSet.Where(e => !e.IsDeleted)
                                           .Where(predicate);

            if (include != null)
                query = include(query);

            return query.FirstOrDefault();
        }

    }
}
