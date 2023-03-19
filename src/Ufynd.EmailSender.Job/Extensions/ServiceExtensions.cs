using Akka.Actor;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Akka.Routing;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Ufynd.Core.Configurations;
using Ufynd.EmailSender.Job.Actors;
using Ufynd.EmailSender.Job.Configurations;
using Ufynd.EmailSender.Job.Services.Interfaces;
using Ufynd.EmailSender.Job.Services.Providers;
using Ufynd.FileUpload.Sdk.Extensions;
using Ufynd.Redis.Sdk.Extensions;

namespace Ufynd.EmailSender.Job.Extensions;

public static class ServiceExtensions
{
    public static void AddCustomServicesAndConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUfyndRedisSdk(c => configuration.GetSection(nameof(RedisConfig)).Bind(c));
        services.AddUfyndFileUploadSdk(c => configuration.GetSection(nameof(S3ServiceConfig)).Bind(c));
        services.Configure<EmailConfiguration>(c => configuration.GetSection(nameof(EmailConfiguration)).Bind(c));
        services.AddSingleton<IEmailService, EmailService>();
    }
    
    public static void AddActorSystem(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        var actorSystem = ActorSystem.Create("UfyndEmailSenderJobActorSystem");
        services.AddSingleton(_ => actorSystem);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);

        containerBuilder.RegisterType<ScheduleEmailActor>();
        containerBuilder.RegisterType<SendEmailActor>();

        var container = containerBuilder.Build();
        var _ = new AutoFacDependencyResolver(container, actorSystem);

        int lowerBound = int.Parse(configuration["ResizeActorConfig:LowerBound"]);
        int upperBound = int.Parse(configuration["ResizeActorConfig:UpperBound"]);
        
        // Create child actors
        ParentActor.ActorSystem = actorSystem;
        ParentActor.ScheduleEmailActor = actorSystem.ActorOf(actorSystem.DI()
            .Props<ScheduleEmailActor>()
            .WithRouter(new SmallestMailboxPool(lowerBound).WithResizer(new DefaultResizer(lowerBound, upperBound)))
            .WithSupervisorStrategy(ParentActor.GetDefaultStrategy()), nameof(ScheduleEmailActor));
        
        ParentActor.SendEmailActor = actorSystem.ActorOf(actorSystem.DI()
            .Props<SendEmailActor>()
            .WithRouter(new SmallestMailboxPool(lowerBound).WithResizer(new DefaultResizer(lowerBound, upperBound)))
            .WithSupervisorStrategy(ParentActor.GetDefaultStrategy()), nameof(SendEmailActor));
        
        
    }
}