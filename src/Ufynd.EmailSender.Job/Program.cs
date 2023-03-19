using Ufynd.EmailSender.Job;
using Ufynd.EmailSender.Job.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddCustomServicesAndConfigurations(hostContext.Configuration);
        services.AddActorSystem(hostContext.Configuration);
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();