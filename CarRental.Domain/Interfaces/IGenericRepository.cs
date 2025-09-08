using CarRental.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Interfaces
{
    /// <summary>
    /// This interface represents the most basic repository of the system.
    /// Any repository interface in this system should be the derived interface of this interface.
    /// </summary>
    public interface IGenericRepository
    {
        
    }

    /// <summary>
    /// Represents a generic repository interface for data access operations,
    /// enabling CRUD functionalities, querying, and other advanced operations
    /// targeted at specific entity types and primary keys.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity for which the repository is created.</typeparam>
    /// <typeparam name="TPrimaryKey">The type of the primary key used to identify the entity.</typeparam>
    public interface IGenericRepository<TEntity, TPrimaryKey> : IGenericRepository where TEntity : BaseEntity<TPrimaryKey>
    {
        // Query methods
        Task<TEntity?> GetByIdAsync(TPrimaryKey id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null);
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null);
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null);
        Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null);

        // Query with includes
        Task<TEntity?> GetByIdAsync(TPrimaryKey id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null, params Expression<Func<TEntity, object>>[] includes);

        // Command methods
        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);
        Task DeleteAsync(TPrimaryKey id);
        Task DeleteAsync(TEntity entity);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities);

        // Soft delete methods
        Task SoftDeleteAsync(TPrimaryKey id);
        Task SoftDeleteAsync(TEntity entity);
        Task SoftDeleteRangeAsync(IEnumerable<TEntity> entities);

        // Pagination
        Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? options = null,
            params Expression<Func<TEntity, object>>[] includes);
    }
}