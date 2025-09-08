using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace CarRental.Domain.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart, Guid>
    {
        public Cart GetFirstOrDefaultWithThenIncluding(
            Expression<Func<Cart, bool>> predicate,
            Func<IQueryable<Cart>, IIncludableQueryable<Cart, object>> include = null);
    }
}
