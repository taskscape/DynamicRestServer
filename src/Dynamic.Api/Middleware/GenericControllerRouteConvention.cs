using Dynamic.Api.Controllers;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Dynamic.Api.Middleware
{
    internal class GenericControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType && controller.ControllerType.GetGenericTypeDefinition() == typeof(GenericController<,,>))
            {
                var entityType = controller.ControllerType.GenericTypeArguments[0];
                controller.ControllerName = entityType.Name.Pluralize();
            }
        }
    }
}
