using Microsoft.AspNetCore.Authorization;

namespace Dynamic.Api.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string TableName { get; }
        public string RoleName { get; }
        public string ActionName { get; }

        public PermissionRequirement()
        {

        }

        public PermissionRequirement(string tableName, string roleName, string actionName)
        {
            TableName = tableName;
            RoleName = roleName;
            ActionName = actionName;
        }
    }
}
