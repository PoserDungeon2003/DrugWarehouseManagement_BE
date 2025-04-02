using DrugWarehouseManagement.Repository.Models;
using System.Linq.Expressions;

namespace DrugWarehouseManagement.Repository.Interface
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        Task<TEntity> GetByIdAsync(object id);
        IQueryable<TEntity> GetByWhere(Expression<Func<TEntity, bool>> predicate);

        Task Detach(TEntity entity);

        Task CreateAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        Task<int> CountAsync();

        Task<IEnumerable<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>> predicate = null);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities);
    }
}
