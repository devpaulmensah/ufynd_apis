using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ufynd.Reporting.Api.Extensions;

namespace Ufynd.Reporting.Api.Tests.TestSetup;

public class TestFixture
{
    public ServiceProvider ServiceProvider { get; }
    
    public TestFixture()
    {
        var services = new ServiceCollection();
        ConfigurationManager.SetupConfiguration();

        services.AddSingleton(sp => ConfigurationManager.Configuration);

        services.AddLogging(x => x.AddConsole());
        services.AddCustomServicesAndConfigurations(ConfigurationManager.Configuration);
        
        ServiceProvider = services.BuildServiceProvider();
    }
}