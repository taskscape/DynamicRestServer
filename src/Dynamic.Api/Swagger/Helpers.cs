using Dynamic.Api.Attributes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Dynamic.Shared;

namespace Dynamic.Api.Swagger
{
    internal static class Helpers
    {
        internal static ApiDescription ResolveActionUsingAttribute(this IEnumerable<ApiDescription> apiDescriptions)
        {
            ApiDescription returnDescription = null;

            foreach (var item in apiDescriptions.Where(f => f.ActionDescriptor.ActionConstraints.Any(a => a is BaseImplemetation)))
            {
                var attr = (BaseImplemetation)item.ActionDescriptor.ActionConstraints.FirstOrDefault(a => a is BaseImplemetation);

                if (item.TryGetMethodInfo(out var methodInfo) && methodInfo.IsOverride() && !attr.IsBaseImplementation)
                {
                    returnDescription = item;
                }
            }

            if (returnDescription is null)
            {
                returnDescription = apiDescriptions
                    .Single(x => x.ActionDescriptor is ControllerActionDescriptor controllerAction && controllerAction.ControllerTypeInfo.BaseType == typeof(ControllerBase));
            }

            return returnDescription;
        }
    }
}
