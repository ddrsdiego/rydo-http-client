namespace Rydo.HttpClient.Configurations
{
    public class CircuitBreakPolicyEntry
    {
        public int DurationOfBreakInSeconds { get; set; }
        public int EventsTimesBeforeBreaking { get; set; }
    }
}