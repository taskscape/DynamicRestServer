using Dynamic.Api.Attributes;
using Dynamic.DAL.Entities;
using Dynamic.Services;
using Dynamic.Services.Dto;
using Dynamic.Shared.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamic.Api.Controllers
{
    [BaseImplemetation(false)]
    public sealed class ConfigurationsController : GenericController<Configuration, ConfigurationDto, ConfigurationEditDto>
    {
        public ConfigurationsController(ConfigurationService service) : base(service)
        {
        }

        public override async Task<ActionResult<IReadOnlyList<ConfigurationDto>>> BrowseAsync(ListQuery listQuery)
        {
            if (!string.IsNullOrWhiteSpace(listQuery.Select))
            {
                return Ok(await Service.BrowseDynamicAsync(listQuery));
            }

            return Ok(await Service.BrowseAsync(listQuery));
        }
    }
}
