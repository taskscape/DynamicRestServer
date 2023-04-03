using Dynamic.Api.Controllers;
using Dynamic.DbScaffolder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamic.Api.Middleware
{
    internal class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var candidates = ScaffolderHelper.GetScaffoldedDbContextEntityTypes();
            var dtos = ScaffolderHelper.GetDtosTypes();

            foreach (var candidate in candidates)
            {
                var dtoType = dtos.Single(x => x.Name == $"{candidate.Name}Dto");
                var editDtoType = dtos.Single(x => x.Name == $"{candidate.Name}EditDto");
                feature.Controllers.Add(typeof(GenericController<,,>).MakeGenericType(candidate, dtoType, editDtoType).GetTypeInfo());
            }
        }
    }
}
