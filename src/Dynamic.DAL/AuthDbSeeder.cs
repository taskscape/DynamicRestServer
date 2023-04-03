using Dynamic.DAL.Entities.Authorization;
using Humanizer;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dynamic.DAL
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Guest = "Guest";
    }

    public class AuthDbSeeder
    {
        private readonly DbContext _scaffoldedDbContext;
        private readonly ILogger<AuthDbSeeder> _logger;
        private readonly CloudTable _azureStorageTable;
        private readonly IMemoryCache _memoryCache;

        public AuthDbSeeder(DbContext scaffoldedDbContext, ILogger<AuthDbSeeder> logger, CloudTable azureStorageTable, IMemoryCache memoryCache)
        {
            _scaffoldedDbContext = scaffoldedDbContext ?? throw new ArgumentNullException(nameof(scaffoldedDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageTable = azureStorageTable ?? throw new ArgumentNullException(nameof(azureStorageTable));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task Seed()
        {
            List<Permission> permissions = null;

            if (await _azureStorageTable.CreateIfNotExistsAsync())
            {
                _logger.LogInformation("Created table named: {tableName}", _azureStorageTable.Name);
                permissions = await InsertPermissions(_azureStorageTable);
            }
            else
            {
                permissions = await GetAllPermissions();
                _logger.LogInformation("Got {Count} permission objects from table '{tableName}'", permissions.Count, _azureStorageTable.Name);
            }

            permissions.ForEach(permission =>
            {
                _memoryCache.Set(permission.RowKey, permission);
            });
            _logger.LogInformation("Permissions stored in memory cache");
        }

        private async Task<List<Permission>> GetAllPermissions()
        {
            List<Permission> result;

            try
            {
                var partitionScanQuery = new TableQuery<Permission>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Permission.PartitionKeyName));

                result = await _azureStorageTable.ExecuteQueryAsync(partitionScanQuery);
            }
            catch (StorageException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }

            return result;
        }

        private async Task<List<Permission>> InsertPermissions(CloudTable cloudTable)
        {
            var roles = new Role[]
            {
                new Role(Roles.Admin),
                new Role(Roles.User),
                new Role(Roles.Guest)
            };
            var httpMethods = new HttpMethod[]
            {
                HttpMethod.Get,
                HttpMethod.Post,
                HttpMethod.Put,
                HttpMethod.Delete
            };

            var tables = GetTables();
            var permissions = new List<Permission>();
            var operationsList = new List<TableBatchOperation>();
            var tasks = new List<Task<TableBatchResult>>();
            var chunkSize = 100;

            foreach (var table in tables)
            {
                foreach (var role in roles)
                {
                    foreach (var httpMethod in httpMethods)
                    {
                        var permission = new Permission(table, role, httpMethod, Enable(role, httpMethod));
                        permissions.Add(permission);
                    }
                }
            }

            while (permissions.Any())
            {
                var batchOperation = new TableBatchOperation();
                var chunk = permissions.Take(chunkSize).ToList();

                chunk.ForEach(entity => batchOperation.Insert(entity));
                operationsList.Add(batchOperation);
                permissions = permissions.Skip(chunkSize).ToList();
            }

            operationsList.ForEach(batch => tasks.Add(cloudTable.ExecuteBatchAsync(batch)));

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(x => x.Select(s => s.Result)).Cast<Permission>().ToList();
        }

        private List<Table> GetTables()
        {
            var tableNames = _scaffoldedDbContext.Model.GetEntityTypes().Select(t => t.GetTableName()).Distinct().ToList();
            return tableNames.Select(tableName => new Table(tableName.Pluralize())).ToList();
        }

        private static bool Enable(Role role, HttpMethod httpMethod) => role.Name switch
        {
            Roles.Admin => true,
            Roles.User => httpMethod == HttpMethod.Get || httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put,
            Roles.Guest => httpMethod == HttpMethod.Get,
            _ => false,
        };
    }
}
