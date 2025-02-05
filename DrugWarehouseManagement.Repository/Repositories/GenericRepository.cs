using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly DrugWarehouseContext _context = null;
        private DbSet<TEntity> table = null;

        public GenericRepository()
        {
            this._context = new DrugWarehouseContext();
            table = _context.Set<TEntity>();
        }

        public GenericRepository(DrugWarehouseContext context)
        {
            this._context = context;
            table = _context.Set<TEntity>();
        }

        public async Task Detach(TEntity entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                return;
            }

            entry.State = EntityState.Detached;
        }

        public async Task CreateAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            // Kiểm tra xem thực thể đã được theo dõi chưa
            var local = _context.Set<TEntity>().Local.FirstOrDefault(e => e == entity);
            if (local != null)
            {
                // Nếu thực thể đã được theo dõi, gỡ bỏ nó
                _context.Entry(local).State = EntityState.Detached;
            }

            // Đính kèm thực thể và đánh dấu nó để xóa
            _context.Set<TEntity>().Attach(entity);
            _context.Entry(entity).State = EntityState.Deleted;
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().AsQueryable().AsNoTracking();
        }

        public async Task<TEntity> GetByIdAsync(object id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public IQueryable<TEntity> GetByWhere(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate).AsNoTracking();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            var local = _context.Set<TEntity>().Local.FirstOrDefault(e => e == entity);
            if (local != null)
            {
                // Nếu thực thể đã được theo dõi, gỡ bỏ nó
                _context.Entry(local).State = EntityState.Detached;
            }

            // Đính kèm thực thể và đánh dấu nó để cập nhật
            _context.Set<TEntity>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<TEntity>().CountAsync();
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>> predicate = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (pageNumber == 0) pageNumber = 1;
            if (pageSize == 0) pageSize = 10;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .AsNoTracking()
                              .ToListAsync();
        }
    }

}
