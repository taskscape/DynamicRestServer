using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Shared.Pagination
{
    public class PaginatedResult<TDto> where TDto : class
    {
        public IEnumerable<TDto> Items { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageCount { get; private set; }
        public int PageSize { get; private set; }
        public int RowCount { get; private set; }

        public PaginatedResult(IEnumerable<TDto> items, int currentPage, int pageCount, int pageSize, int rowCount)
        {
            Items = items;
            CurrentPage = currentPage;
            PageCount = pageCount;
            PageSize = pageSize;
            RowCount = rowCount;
        }

        public static PaginatedResult<TDto> From(PagedResult result, IEnumerable<TDto> items)
            => new(items, result.CurrentPage, result.PageCount, result.PageSize, result.RowCount);

        public static async Task<PaginatedResult<TDto>> CreateAsync<TEntity>(PagedResult<TEntity> result, IMapper mapper) where TEntity : class
        {
            var entities = await result.Queryable.ToListAsync();
            var items = mapper.Map<IEnumerable<TDto>>(entities);

            return new PaginatedResult<TDto>(items, result.CurrentPage, result.PageCount, result.PageSize, result.RowCount);
        }
    }
}
