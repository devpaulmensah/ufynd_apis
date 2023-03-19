using Akka.Actor;
using Newtonsoft.Json;
using Ufynd.Core.Constants;
using Ufynd.EmailSender.Job.Actors;
using Ufynd.EmailSender.Job.Actors.Messages;
using Ufynd.Redis.Sdk.Services.Interfaces;

namespace Ufynd.EmailSender.Job;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRedisService _redisService;

    public Worker(ILogger<Worker> logger,
        IConfiguration configuration,
        IRedisService redisService)
    {
        _logger = logger;
        _configuration = configuration;
        _redisService = redisService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int fetchTimeIntervalInMinutes = _configuration.GetValue<int>("FetchTimeIntervalInMinutes");

            /*
             * Get all emails that will be sent before *fetchTimeIntervalInMinutes* from now.
             * This will enable us to set timers for the emails in our actors 
             */
            DateTime fetchScheduledBeforeTime = DateTime.UtcNow.AddMinutes(fetchTimeIntervalInMinutes);
            
            _logger.LogInformation("Getting all automated emails scheduled before {time}", fetchScheduledBeforeTime);

            List<CacheMessage> emailsToBeSentBeforeScheduledTime = await GetAutomatedEmailsToBeSentWithinTheNext5Minutes(fetchScheduledBeforeTime);
            
            _logger.LogInformation("Fetched {scheduledEmailsCount} to be sent before {time}. Processing...", 
                emailsToBeSentBeforeScheduledTime.Count, fetchScheduledBeforeTime);
            
            emailsToBeSentBeforeScheduledTime.ForEach(cacheMessage => ParentActor.ScheduleEmailActor.Tell(new CacheMessage
            {
                ScheduledTime = cacheMessage.ScheduledTime,
                Filename = cacheMessage.Filename,
                EmailSubject = cacheMessage.EmailSubject,
                RecipientEmailAddress = cacheMessage.RecipientEmailAddress
            }, ActorRefs.Nobody));
            
            await Task.Delay(TimeSpan.FromMinutes(fetchTimeIntervalInMinutes), stoppingToken);
        }
    }

    private async Task<List<CacheMessage>> GetAutomatedEmailsToBeSentWithinTheNext5Minutes(DateTime fetchScheduledBeforeTime)
    {
        try
        {
            return (await _redisService.GetRedisClient().HashGetAllAsync(RedisConstants.ScheduledEmailsKey))
                .Where(x => 
                    JsonConvert.DeserializeObject<CacheMessage>(x.Value.ToString()).ScheduledTime <= fetchScheduledBeforeTime)
                .Select(x => JsonConvert.DeserializeObject<CacheMessage>(x.Value.ToString()))
                .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured getting automated emails to be sent within the next 5 minutes from redis");
            return new List<CacheMessage>();
        }
        
    }
}