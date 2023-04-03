using Dynamic.Shared.Pagination;
using Dynamic.Shared.Queries;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Dynamic.DAL.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        PagedResult<T> GetPagedResultAsync(PaginatedQuery query);
        Task<IReadOnlyList<T>> GetAllAsync(ListQuery listQuery);
        Task<IReadOnlyList<dynamic>> GetAllDynamicAsync(ListQuery listQuery);
        Task<T> GetAsync(int id, GetQuery getQuery = null);
        Task<dynamic> GetDynamicAsync(int id, GetQuery getQuery);
        Task<int> AddAsync(T entityToAdd);
        Task DeleteAsync(T entityToDelete);
        Task UpdateAsync(T entity);
    }
}
