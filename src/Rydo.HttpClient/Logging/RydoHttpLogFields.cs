namespace Rydo.HttpClient.Logging
{
    internal static class RydoHttpLogFields
    {
        public const string PolicyKey = "{PolicyKey}";
        public const string EndpointName = "{EndpointName}";
        public const string LogType = "{LogType}";
        public const string Uri = "{Uri}";
        public const string StatusCode = "{StatusCode}";
        public const string HttpMethod = "{HttpMethod}";
        public const string RetryAttempt = "{RetryAttempt}";
        public const string CorrelationId = "{cid}";
        public const string ElapsedMilliseconds = "{ElapsedMilliseconds}";
        public const string WaitingTimeSpan = "{WaitingTimeSpan}";
    }
}