using Dynamic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dynamic.Api.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IAuthService _authService;

        public PermissionHandler(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var roleName = context.User.FindFirstValue(ClaimTypes.Role);

            if (roleName is null)
            {
                context.Fail();
            }
            else if (context.Resource is AuthorizationFilterContext filterContext && filterContext.ActionDescriptor is ControllerActionDescriptor controllerAction)
            {
                var tableName = controllerAction.ControllerName.ToLower();
                var httpMethod = filterContext.HttpContext.Request.Method;
                var isAuthorized = _authService.IsAuthorized(tableName, roleName, httpMethod);

                if (isAuthorized)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
