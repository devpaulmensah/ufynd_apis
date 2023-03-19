using Akka.Actor;
using Akka.Routing;
using Ufynd.Core.Configurations;

namespace Ufynd.Reporting.Api.Actors;

public static class ParentActor
{
    public static IActorRef CacheActor = ActorRefs.Nobody;
    public static IActorRef UploadActor = ActorRefs.Nobody;
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