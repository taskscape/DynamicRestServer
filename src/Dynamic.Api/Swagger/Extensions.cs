using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dynamic.Api.Swagger
{
    internal static class Extensions
    {
        internal static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dynamic.Api", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.ResolveActionUsingAttribute());

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.EnableAnnotations();
                c.CustomOperationIds(x =>
                    $"{x.HttpMethod?[0]}{x.HttpMethod?.Substring(1).ToLower()}{x.RelativePath.Replace("api/", "").Replace("/{id}", x.HttpMethod == "GET" ? "ById" : "").Replace("/paged", "Paged")}"
                );

                c.CustomSchemaIds(DefaultSchemaIdSelector);
            });

            return services;
        }

        internal static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dynamic.Api v1"));

            return app;
        }

        private static string DefaultSchemaIdSelector(Type modelType)
        {
            if (!modelType.IsConstructedGenericType)
            {
                return modelType.FullName;
            }

            var prefix = modelType.GetGenericArguments()
                .Select(genericArg => DefaultSchemaIdSelector(genericArg))
                .Aggregate((previous, current) => previous + current);

            return $"{prefix}{modelType.FullName.Split('`').First()}";
        }
    }
}
