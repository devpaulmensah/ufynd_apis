using Akka.Actor;
using Newtonsoft.Json;
using Ufynd.Core.Constants;
using Ufynd.Redis.Sdk.Services.Interfaces;
using Ufynd.Reporting.Api.Actors.Messages;

namespace Ufynd.Reporting.Api.Actors;

public class CacheActor : ReceiveActor
{
    private readonly ILogger<CacheActor> _logger;
    private readonly IRedisService _redisService;

    public CacheActor(ILogger<CacheActor> logger,
        IRedisService redisService)
    {
        _logger = logger;
        _redisService = redisService;
        
        ReceiveAsync<CacheMessage>(CacheEmailTrigger);
    }

    private async Task CacheEmailTrigger(CacheMessage message)
    {
        try
        {
            bool isSaved = await _redisService.GetRedisClient().HashSetAsync(
                key: RedisConstants.ScheduledEmailsKey, 
                hashField: message.Filename, 
                value: JsonConvert.SerializeObject(message));

            if (!isSaved)
            {
                _logger.LogError("An error occured saving scheduled email in redis\n{emailTrigger}",
                    JsonConvert.SerializeObject(message));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured caching email trigger\n{emailTrigger}",
                JsonConvert.SerializeObject(message));
        }
    }
}