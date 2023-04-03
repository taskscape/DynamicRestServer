using Dynamic.DAL;
using Dynamic.DbScaffolder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace Dynamic.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables()
                  .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var runtimeScaffolderlogger = services.GetRequiredService<ILogger<RuntimeScaffolder>>();
                    var config = host.Services.GetRequiredService<IConfiguration>();
                    var scaffolder = new RuntimeScaffolder(runtimeScaffolderlogger, config.GetConnectionString("DynamicDbContext"), false);

                    scaffolder.Start();

                    var azureStorageTable = services.GetRequiredService<CloudTable>();
                    var scaffoldedDbContext = services.GetRequiredService<DbContext>();
                    var authDbSeederlogger = services.GetRequiredService<ILogger<AuthDbSeeder>>();
                    var memoryCache = services.GetRequiredService<IMemoryCache>();
                    var authDbSeeder = new AuthDbSeeder(scaffoldedDbContext, authDbSeederlogger, azureStorageTable, memoryCache);
                    authDbSeeder.Seed().Wait();
                }

                Log.Information("Starting web host");
                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
