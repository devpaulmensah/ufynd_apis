using StackExchange.Redis;

namespace Ufynd.Redis.Sdk.Services.Interfaces;

public interface IRedisService
{
    IDatabase GetRedisClient();
}