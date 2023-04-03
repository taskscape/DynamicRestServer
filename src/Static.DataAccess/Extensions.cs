using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Static.DataAccess
{
    public static class Extensions
    {
        public static IServiceCollection AddPowerAppDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<PowerAppDataContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            return services;
        }
    }
}
