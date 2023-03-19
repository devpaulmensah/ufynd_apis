using Microsoft.Extensions.DependencyInjection;
using Ufynd.Core.Configurations;
using Ufynd.FileUpload.Sdk.Services.Interfaces;
using Ufynd.FileUpload.Sdk.Services.Providers;

namespace Ufynd.FileUpload.Sdk.Extensions;

public static class ServiceExtensions
{
    public static void AddUfyndFileUploadSdk(this IServiceCollection services, Action<S3ServiceConfig> s3Config)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        
        services.Configure(s3Config);
        services.AddSingleton<IUploadService, UploadService>();
    }
}