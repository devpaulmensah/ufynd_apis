using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Ufynd.Core.Configurations;
using Ufynd.Redis.Sdk.Services.Interfaces;
using Ufynd.Redis.Sdk.Services.Providers;

namespace Ufynd.Redis.Sdk.Extensions;

public static class ServiceExtensions
{
    public static void AddUfyndRedisSdk(this IServiceCollection services, Action<RedisConfig> redisConfig)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        services.Configure(redisConfig);

        RedisConfig redisConfiguration = new RedisConfig();
        redisConfig.Invoke(redisConfiguration);
        
        var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { redisConfiguration.BaseUrl },
            AllowAdmin = true,
            AbortOnConnectFail = false,
            ReconnectRetryPolicy = new LinearRetry(500),
            DefaultDatabase = redisConfiguration.Database
        });

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        services.AddSingleton<IRedisService, RedisService>();

    }
}