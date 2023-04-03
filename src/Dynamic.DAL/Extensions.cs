using Dynamic.DAL.Repositories;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dynamic.DAL
{
    public static class Extensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IEfGenericRepository<,>), typeof(EfGenericRepository<,>));

            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<CustomDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            return services;
        }

        public static IServiceCollection AddAzureStorage(this IServiceCollection services, string connectionString, string tableName)
        {
            services.AddSingleton<CloudTable>((x) =>
            {
                var storageAccount = CreateStorageAccount(connectionString);
                var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

                return tableClient.GetTableReference(tableName);
            });

            return services;
        }

        private static CloudStorageAccount CreateStorageAccount(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return CloudStorageAccount.Parse(connectionString);
        }
    }
}
