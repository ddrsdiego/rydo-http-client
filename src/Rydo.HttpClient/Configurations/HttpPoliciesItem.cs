namespace Rydo.HttpClient.Configurations
{
    public class HttpPoliciesItem
    {
        public int TimeoutInSeconds { get; set; }

        public RetryPolicyEntry? Retry { get; set; }

        public CircuitBreakPolicyEntry? CircuitBreak { get; set; }
    }
}