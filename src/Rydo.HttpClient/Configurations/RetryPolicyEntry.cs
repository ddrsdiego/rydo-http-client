namespace Rydo.HttpClient.Configurations
{
    public sealed class RetryPolicyEntry
    {
        public int NumberOfRetries { get; set; } = 3;
        public int FirstRetryWaitTimeInSeconds { get; set; } = 1;
        public int RetryProgressionBaseValueInSeconds { get; set; } = 1;
        public TimeProgressionType ProgressionType { get; set; } = TimeProgressionType.None;
    }
}