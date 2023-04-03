using AutoMapper;
using Dynamic.DAL;
using Dynamic.DAL.Entities;
using Dynamic.DAL.Repositories;
using Dynamic.Services.Dto;
using Dynamic.Shared.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamic.Services
{
    public sealed class ConfigurationService : GenericService<Configuration, ConfigurationDto, ConfigurationEditDto>
    {
        public ConfigurationService(IEfGenericRepository<Configuration, CustomDbContext> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public override async Task<IReadOnlyList<ConfigurationDto>> BrowseAsync(ListQuery listQuery)
        {
            var list = await Repository.GetAllAsync(listQuery);
            return Mapper.Map<IReadOnlyList<ConfigurationDto>>(list);
        }
    }
}
