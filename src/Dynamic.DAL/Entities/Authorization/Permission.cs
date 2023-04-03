using Microsoft.Azure.Cosmos.Table;
using System.Net.Http;

namespace Dynamic.DAL.Entities.Authorization
{
    public class Permission : TableEntity
    {
        public static string PartitionKeyName { get; } = "Auth";
        public bool Enabled { get; set; }

        public Permission()
        {

        }

        public Permission(Table table, Role role, HttpMethod httpMethod, bool enabled)
        {
            PartitionKey = PartitionKeyName;
            RowKey = $"{table.EntityName}_{role.Name}_{httpMethod.Method}";
            Enabled = enabled;
        }
    }
}
