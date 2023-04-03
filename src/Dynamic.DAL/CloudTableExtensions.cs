using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamic.DAL
{
    public static class CloudTableExtensions
    {
        public static async Task<List<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query) where T : ITableEntity, new()
        {
            var result = new List<T>();

            TableContinuationToken continuationToken = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                result.AddRange(segment.Results);
                continuationToken = segment.ContinuationToken;
            } while (continuationToken is not null);

            return result;
        }
    }
}
