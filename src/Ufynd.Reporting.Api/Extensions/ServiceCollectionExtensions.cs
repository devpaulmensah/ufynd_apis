using System.Reflection;
using Akka.Actor;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Akka.Routing;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Ufynd.Core.Configurations;
using Ufynd.FileUpload.Sdk.Extensions;
using Ufynd.Redis.Sdk.Extensions;
using Ufynd.Reporting.Api.Actors;
using Ufynd.Reporting.Api.Services.Interfaces;
using Ufynd.Reporting.Api.Services.Providers;

namespace Ufynd.Reporting.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ufynd Reporting Api",
                Version = "v1",
                Description = "Official Documentation for Ufynd Reporting Api",
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

    public static void AddCustomServicesAndConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // Services
        services.AddUfyndFileUploadSdk(c => configuration.GetSection(nameof(S3ServiceConfig)).Bind(c));
        services.AddUfyndRedisSdk(c => configuration.GetSection(nameof(RedisConfig)).Bind(c));
        services.AddScoped<IReportingService, ReportingService>();
    }

    public static void AddActorSystem(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        var actorSystem = ActorSystem.Create("UfyndReportingActorSystem");
        services.AddSingleton(_ => actorSystem);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);

        containerBuilder.RegisterType<FileUploadActor>();
        containerBuilder.RegisterType<CacheActor>();

        var container = containerBuilder.Build();
        var _ = new AutoFacDependencyResolver(container, actorSystem);

        int lowerBound = int.Parse(configuration["ResizeActorConfig:LowerBound"]);
        int upperBound = int.Parse(configuration["ResizeActorConfig:UpperBound"]);
        
        // Create child actors
        ParentActor.ActorSystem = actorSystem;
        ParentActor.UploadActor = actorSystem.ActorOf(actorSystem.DI()
            .Props<FileUploadActor>()
            .WithRouter(new SmallestMailboxPool(lowerBound).WithResizer(new DefaultResizer(lowerBound, upperBound)))
            .WithSupervisorStrategy(ParentActor.GetDefaultStrategy()), nameof(FileUploadActor));
        
        ParentActor.CacheActor = actorSystem.ActorOf(actorSystem.DI()
            .Props<CacheActor>()
            .WithRouter(new SmallestMailboxPool(lowerBound).WithResizer(new DefaultResizer(lowerBound, upperBound)))
            .WithSupervisorStrategy(ParentActor.GetDefaultStrategy()), nameof(CacheActor));
    }
    
    
}