using Dynamic.Api.Authorization;
using Dynamic.Api.Middleware;
using Dynamic.Api.Swagger;
using Dynamic.DAL;
using Dynamic.DbScaffolder;
using Dynamic.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Serilog;
using Static.DataAccess;

namespace Dynamic.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddControllers(options =>
            {
                options.Conventions.Add(new GenericControllerRouteConvention());
                options.SuppressAsyncSuffixInActionNames = false;

                var policy = new AuthorizationPolicyBuilder()
                       .AddRequirements(new PermissionRequirement())
                       .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).ConfigureApplicationPartManager(m => m.FeatureProviders.Add(new GenericControllerFeatureProvider()))
            .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddSwagger();

            services.AddScaffoldedDbContext();
            services.AddCustomDbContext(Configuration.GetConnectionString("DynamicDbContext"));
            services.AddAzureStorage(Configuration.GetConnectionString("AzureTableStorage"), Configuration.GetValue<string>("AzureTableStorageName"));
            services.AddRepositories();
            services.AddPowerAppDbContext(Configuration.GetConnectionString("DynamicDbContext"));

            services.AddServices();
            services.AddAutoMapper();

            services.AddSingleton<ErrorHandlerMiddleware>();

            services.AddMicrosoftIdentityWebApiAuthentication(Configuration);

            services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.RoleClaimType = "roles";
            });

            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                logger.LogInformation("Environment: Development");
            }
            else
            {
                logger.LogInformation("Environment: Production");
            }

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseSwaggerWithUI();

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
