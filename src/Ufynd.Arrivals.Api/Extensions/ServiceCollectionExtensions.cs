using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Ufynd.Arrivals.Api.Services.Interfaces;
using Ufynd.Arrivals.Api.Services.Providers;

namespace Ufynd.Arrivals.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ufynd Arrivals Api",
                Version = "v1",
                Description = "Official Documentation for Ufynd Arrivals Api",
                Contact = new OpenApiContact
                {
                    Name = "Paul Mensah",
                    Email = "paulmensah1409@gmail.com"
                }
            });
            
            c.ResolveConflictingActions(resolver => resolver.First());
            c.EnableAnnotations();
            
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    public static void AddCustomServicesAndConfigurations(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IArrivalService, ArrivalService>();
    }
}