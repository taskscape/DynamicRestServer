using Dynamic.Shared.Pagination;
using Dynamic.Shared.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamic.Services
{
    public interface IGenericService<TEntity, TDto, TEditDto> where TEntity : class where TDto : class where TEditDto : class
    {
        Task<PaginatedResult<TDto>> GetPagedResultAsync(PaginatedQuery query);
        Task<IReadOnlyList<TDto>> BrowseAsync(ListQuery listQuery);
        Task<IReadOnlyList<dynamic>> BrowseDynamicAsync(ListQuery listQuery);
        Task<TDto> GetAsync(int id, GetQuery getQuery);
        Task<dynamic> GetDynamicAsync(int id, GetQuery getQuery);
        Task<int> AddAsync(TEditDto editDto);
        Task UpdateAsync(int id, TEditDto editDto);
        Task DeleteAsync(int id);
    }
}