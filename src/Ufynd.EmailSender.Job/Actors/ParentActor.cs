using Akka.Actor;

namespace Ufynd.EmailSender.Job.Actors;

public class ParentActor
{
    public static IActorRef SendEmailActor = ActorRefs.Nobody;
    public static IActorRef ScheduleEmailActor = ActorRefs.Nobody;
    public static ActorSystem ActorSystem;

    public static SupervisorStrategy GetDefaultStrategy() =>
        new OneForOneStrategy(
            maxNrOfRetries: 3,
            withinTimeRange: TimeSpan.FromSeconds(3),
            localOnlyDecider: exception =>
            {
                if (exception is not ActorInitializationException)
                {
                    return Directive.Resume;
                }

                ActorSystem.Terminate().Wait(1000);
                return Directive.Stop;
            });
}