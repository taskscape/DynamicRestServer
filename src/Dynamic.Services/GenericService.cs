using AutoMapper;
using Dynamic.DAL.Repositories;
using Dynamic.Shared.Exceptions;
using Dynamic.Shared.Pagination;
using Dynamic.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamic.Services
{
    public class GenericService<TEntity, TDto, TEditDto> : IGenericService<TEntity, TDto, TEditDto> where TEntity : class where TDto : class where TEditDto : class
    {
        protected readonly IGenericRepository<TEntity> Repository;
        protected readonly IMapper Mapper;

        public GenericService(IEfGenericRepository<TEntity, DbContext> repository, IMapper mapper)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<TDto>> GetPagedResultAsync(PaginatedQuery query)
        {
            var pagedResult = Repository.GetPagedResultAsync(query);
            return await PaginatedResult<TDto>.CreateAsync(pagedResult, Mapper);
        }

        public virtual async Task<IReadOnlyList<TDto>> BrowseAsync(ListQuery listQuery)
        {
            var list = await Repository.GetAllAsync(listQuery);
            return Mapper.Map<IReadOnlyList<TDto>>(list);
        }

        public virtual async Task<IReadOnlyList<dynamic>> BrowseDynamicAsync(ListQuery listQuery) => await Repository.GetAllDynamicAsync(listQuery);

        public virtual async Task<TDto> GetAsync(int id, GetQuery getQuery)
        {
            var entity = await Repository.GetAsync(id, getQuery);

            if (entity is not null)
            {
                return Mapper.Map<TDto>(entity);
            }

            return null;
        }

        public virtual async Task<dynamic> GetDynamicAsync(int id, GetQuery getQuery) => await Repository.GetDynamicAsync(id, getQuery);

        public virtual async Task<int> AddAsync(TEditDto editDto)
        {
            var entity = Mapper.Map<TEntity>(editDto);
            return await Repository.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(int id, TEditDto editDto)
        {
            var entity = await Repository.GetAsync(id);

            if (entity is null)
            {
                throw new EntityNotFoundException(id, typeof(TEntity));
            }

            Mapper.Map(editDto, entity);

            await Repository.UpdateAsync(entity);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await Repository.GetAsync(id);

            if (entity is null)
            {
                throw new EntityNotFoundException(id, typeof(TEntity));
            }

            await Repository.DeleteAsync(entity);
        }
    }
}
