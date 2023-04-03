using Dynamic.Shared;
using Dynamic.Shared.Pagination;
using Dynamic.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Dynamic.DAL.Repositories
{
    public class EfGenericRepository<T, TDbContext> : IEfGenericRepository<T, TDbContext> where T : class where TDbContext : DbContext
    {
        private readonly DbSet<T> _dbSet;

        public TDbContext DbContext { get; private set; }

        public EfGenericRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
            _dbSet = DbContext.Set<T>();
        }

        public async Task<int> AddAsync(T entityToAdd)
        {
            await _dbSet.AddAsync(entityToAdd);
            await DbContext.SaveChangesAsync();

            return entityToAdd.GetIdValue();
        }

        public PagedResult<T> GetPagedResultAsync(PaginatedQuery query)
        {
            return _dbSet
                .AsNoTracking()
                .ApplyOrderBy(query)
                .PageResult(query.Page, query.PageSize);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(ListQuery listQuery)
        {
            var query = _dbSet
               .AsNoTracking()
               .ApplyWhere(listQuery)
               .ApplyOrderBy(listQuery)
               .ApplySkip(listQuery)
               .ApplyTake(listQuery)
               .ApplyInclude(listQuery);

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<dynamic>> GetAllDynamicAsync(ListQuery listQuery)
        {
            var query = _dbSet
                .AsNoTracking()
                .ApplyWhere(listQuery)
                .ApplyOrderBy(listQuery)
                .ApplySkip(listQuery)
                .ApplyTake(listQuery)
                .ApplyInclude(listQuery)
                .ApplySelect(listQuery);

            return await query.ToDynamicListAsync();
        }

        public async Task<T> GetAsync(int id, GetQuery getQuery)
        {
            if (!string.IsNullOrWhiteSpace(getQuery?.Include))
            {
                var keyProperty = GetKeyPropertyName();
                return await _dbSet
                    .AsNoTracking()
                    .ApplyInclude(getQuery)
                    .SingleOrDefaultAsync(e => EF.Property<int>(e, keyProperty) == id);
            }

            return await _dbSet.FindAsync(id);
        }

        public async Task<dynamic> GetDynamicAsync(int id, GetQuery getQuery)
        {
            var keyPropertyName = GetKeyPropertyName();
            var query = _dbSet
                .AsNoTracking()
                .Where(e => EF.Property<int>(e, keyPropertyName) == id)
                .ApplyInclude(getQuery)
                .ApplySelect(getQuery);

            return await query.SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            await DbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entityToDelete)
        {
            _dbSet.Remove(entityToDelete);
            await DbContext.SaveChangesAsync();
        }

        private string GetKeyPropertyName() => DbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0].Name;
    }
}
