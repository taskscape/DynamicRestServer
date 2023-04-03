using Dynamic.Shared.Pagination;
using Dynamic.Shared.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamic.DAL.Repositories
{
    public class InMemoryGenericRepository<T> : IGenericRepository<T> where T : class, IIdentifiable, new()
    {
        private readonly List<T> _list = new() { new T(), new T() };

        public async Task<int> AddAsync(T entityToAdd)
        {
            _list.Add(entityToAdd);
            await Task.CompletedTask;

            return entityToAdd.Id;
        }

        public Task DeleteAsync(T entityToDelete)
        {
            _list.Remove(entityToDelete);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(string expand)
        {
            await Task.CompletedTask;
            return _list;
        }

        public Task<IReadOnlyList<T>> GetAllAsync(ListQuery listQuery)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyList<dynamic>> GetAllDynamicAsync(ListQuery listQuery)
        {
            throw new System.NotImplementedException();
        }

        public Task<T> GetAsync(int id, string expand)
            => Task.FromResult(_list.SingleOrDefault(x => x.Id == id));

        public Task<T> GetAsync(int id, GetQuery getQuery = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<dynamic> GetDynamicAsync(int id, GetQuery getQuery)
        {
            throw new System.NotImplementedException();
        }

        public System.Linq.Dynamic.Core.PagedResult<T> GetPagedResultAsync(PaginatedQuery query)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateAsync(T entity) => Task.CompletedTask;
    }
}
