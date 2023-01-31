namespace Rydo.HttpClient.Logging
{
    using Microsoft.Extensions.Logging;

    internal static class EventIds
    {
        public static readonly EventId RequestStarted = new EventId(100, "request-pipeline-started");
        public static readonly EventId RequestEnded = new EventId(101, "request-pipeline-ended");
        public static readonly EventId RequestRetried = new EventId(110, "request-policy-retried");
        public static readonly EventId RequestOpenedCircuit = new EventId(111, "request-policy_opened-circuit");
        public static readonly EventId RequestClosedCircuit = new EventId(112, "request-policy_closed-circuit");
    }
}