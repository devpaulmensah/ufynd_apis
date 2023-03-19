using StackExchange.Redis;
using Ufynd.Redis.Sdk.Services.Interfaces;

namespace Ufynd.Redis.Sdk.Services.Providers;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public IDatabase GetRedisClient() =>
        _connectionMultiplexer.GetDatabase();
}