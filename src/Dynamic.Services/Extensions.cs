using Dynamic.Services.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Services
{
    public static class Extensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericService<,,>), typeof(GenericService<,,>));
            services.AddScoped(typeof(ConfigurationService));
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(GenericMappings));

            return services;
        }
    }
}
