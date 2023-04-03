using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Dynamic.DbScaffolder
{
    public static class Extensions
    {
        public static IServiceCollection AddScaffoldedDbContext(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var scaffoldedDbContextType = ScaffolderHelper.GetScaffoldedDbContextType();
                var logger = provider.GetRequiredService<ILoggerFactory>();
                var dbContext = (DbContext)Activator.CreateInstance(scaffoldedDbContextType, logger);

                return dbContext;
            });

            return services;
        }
    }
}
