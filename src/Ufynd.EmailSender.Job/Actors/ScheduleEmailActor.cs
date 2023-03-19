using Akka.Actor;
using Newtonsoft.Json;
using Ufynd.EmailSender.Job.Actors.Messages;

namespace Ufynd.EmailSender.Job.Actors;

public class ScheduleEmailActor : ReceiveActor
{
    private readonly ILogger<ScheduleEmailActor> _logger;

    public ScheduleEmailActor(ILogger<ScheduleEmailActor> logger)
    {
        _logger = logger;

        Receive<CacheMessage>(ScheduleEmail);
    }

    private void ScheduleEmail(CacheMessage message)
    {
        try
        {
            _logger.LogInformation("Scheduling email to be sent at {sendTime}", message.ScheduledTime);

            TimeSpan timeSpan = message.ScheduledTime.Subtract(DateTime.UtcNow);

            // Check if time has already passed and reset to send email 5 seconds from now
            if (timeSpan.Milliseconds < 1)
            {
                timeSpan = TimeSpan.FromSeconds(5);
            }

            Context.System.Scheduler.ScheduleTellOnce(timeSpan, ParentActor.SendEmailActor, new CacheMessage
            {
                ScheduledTime = message.ScheduledTime,
                Filename = message.Filename,
                EmailSubject = message.EmailSubject,
                RecipientEmailAddress = message.RecipientEmailAddress
            }, ActorRefs.Nobody);
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured scheduling email to be sent\n{emailDetails}", 
                JsonConvert.SerializeObject(message));
        }   
    }
}