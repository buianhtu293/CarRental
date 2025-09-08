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

namespace CarRental.Infrastructure.Repositories
{
    public class GenericRepository<TEntity, TPrimaryKey> : IGenericRepository<TEntity, TPrimaryKey>
        where TEntity : BaseEntity<TPrimaryKey>
    {
        protected readonly CarRentalDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(CarRentalDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        #region Query Methods

        public virtual async Task<TEntity?> GetByIdAsync(TPrimaryKey id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id) && !e.IsDeleted);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            return await query.Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            return await query.Where(predicate).Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            return await query.Where(predicate).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            return await query.Where(predicate).Where(e => !e.IsDeleted).AnyAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null)
        {
            var query = ApplyQueryOptions(_dbSet, options);

            if (predicate == null)
            {
                return await query.Where(e => !e.IsDeleted).CountAsync();
            }

            return await query.Where(predicate).Where(e => !e.IsDeleted).CountAsync();
        }

        #endregion

        #region Query Methods with Includes

        public virtual async Task<TEntity?> GetByIdAsync(TPrimaryKey id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id) && !e.IsDeleted);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            return await query.Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            return await query.Where(predicate).Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = ApplyQueryOptions(_dbSet, options);
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            return await query.Where(predicate).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
        }

        #endregion

        #region Command Methods

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsDeleted = false;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var entityList = entities.ToList();
            foreach (var entity in entityList)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.IsDeleted = false;
            }

            await _dbSet.AddRangeAsync(entityList);
            return entityList;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var entityList = entities.ToList();
            foreach (var entity in entityList)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }

            _dbSet.UpdateRange(entityList);
            return await Task.FromResult(entityList);
        }

        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        #endregion

        #region Soft Delete Methods

        public virtual async Task SoftDeleteAsync(TPrimaryKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(entity);
            }
        }

        public virtual async Task SoftDeleteAsync(TEntity entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(entity);
        }

        public virtual async Task SoftDeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            var entityList = entities.ToList();
            foreach (var entity in entityList)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            await UpdateRangeAsync(entityList);
        }

        #endregion

        #region Pagination

        public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = ApplyQueryOptions(_dbSet, options);

            // Apply includes
            query = includes.Aggregate(query, (current, include) => current.Include(include));

            // Apply soft delete filter
            query = query.Where(e => !e.IsDeleted);

            // Apply predicate if provided
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Applies custom query options to a given queryable collection in a conditional and safe manner.
        /// </summary>
        /// <param name="query">The initial queryable collection to which options will be applied.</param>
        /// <param name="options">An optional function that defines the query customization logic. If null, the query remains unchanged.</param>
        /// <returns>The queryable collection after applying the specified options, or the original query if no options are provided.</returns>
        protected virtual IQueryable<TEntity> ApplyQueryOptions(
            IQueryable<TEntity> query,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null)
        {
            return options != null ? options(query) : query;
        }

        #endregion
    }
}