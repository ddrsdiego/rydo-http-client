namespace Rydo.HttpClient.Policies
{
    using System;
    using System.Net.Http;
    using Configurations;
    using Logging;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Extensions.Http;
    using Polly.Timeout;

    public static class WaitAndRetryPolicyFactory
    {
        private const string PolicyType = "retry";

        public static IAsyncPolicy<HttpResponseMessage> GetWaitAndRetryPolicy(this EndpointEntry endpointEntry)
        {
            Helper.GuardNull(endpointEntry, nameof(endpointEntry));

            var policy = endpointEntry.Retry is null
                ? Policy.NoOpAsync<HttpResponseMessage>()
                : CreateAsyncPolicy(endpointEntry);

            return policy;
        }

        private static IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy(EndpointEntry endpointEntry)
        {
            var policyKey = $"{endpointEntry.Name}-{PolicyType}";

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(endpointEntry.Retry!.NumberOfRetries,
                    retryAttempt => WaitStrategyRetries(retryAttempt, endpointEntry),
                    onRetry: OnHttpRetry)
                .WithPolicyKey(policyKey);
        }

        private static void OnHttpRetry(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount,
            Context context)
        {
            if (IsInvalidContext()) return;

            RegisterLogForRetry(result, context, timeSpan, retryCount);

            bool IsInvalidContext() => context.Count == 0;
        }

        private static void RegisterLogForRetry(DelegateResult<HttpResponseMessage> result, Context context, TimeSpan timeSpan, int retryCount)
        {
            var correlationId = context[Constants.XCorrelationId].ToString();
            var endpointName = context[Constants.EndpointEntryName].ToString();
            var logger = context[Constants.PolicyContextLogger] as ILogger<HttpServiceRequester>;

            if (result.Result != null)
            {
                logger?.LogWarning(EventIds.RequestRetried,
                    $"{RydoHttpLogFields.LogType} {RydoHttpLogFields.EndpointName} {RydoHttpLogFields.CorrelationId} Request failed with {RydoHttpLogFields.StatusCode}. Waiting {RydoHttpLogFields.WaitingTimeSpan} before next retry. Retry attempt {RydoHttpLogFields.RetryAttempt}",
                    RydoHttpLogTypes.HttpRequestOut,
                    endpointName,
                    correlationId,
                    result.Result.StatusCode,
                    timeSpan,
                    retryCount);                
            }
            else
            {
                logger?.LogWarning(EventIds.RequestRetried,
                    $"{RydoHttpLogFields.LogType} {RydoHttpLogFields.EndpointName} {RydoHttpLogFields.CorrelationId} Request failed because network failure. Waiting {RydoHttpLogFields.WaitingTimeSpan} before next retry. Retry attempt {RydoHttpLogFields.RetryAttempt}",
                    RydoHttpLogTypes.HttpRequestOut,
                    endpointName,
                    correlationId,
                    timeSpan,
                    retryCount);
            }
        }

        internal static TimeSpan WaitStrategyRetries(int retryAttempt, EndpointEntry endpointEntry)
        {
            if (endpointEntry == null) throw new ArgumentNullException(nameof(endpointEntry));

            var value = endpointEntry.Retry!.ProgressionType switch
            {
                TimeProgressionType.None => endpointEntry.Retry.FirstRetryWaitTimeInSeconds +
                                            endpointEntry.Retry.RetryProgressionBaseValueInSeconds,

                TimeProgressionType.Arithmetic => endpointEntry.Retry.FirstRetryWaitTimeInSeconds +
                                                  (retryAttempt * endpointEntry.Retry
                                                      .RetryProgressionBaseValueInSeconds),

                TimeProgressionType.Geometric => endpointEntry.Retry.FirstRetryWaitTimeInSeconds +
                                                 Math.Pow(endpointEntry.Retry.RetryProgressionBaseValueInSeconds,
                                                     retryAttempt),

                _ => endpointEntry.Retry.FirstRetryWaitTimeInSeconds +
                     endpointEntry.Retry.RetryProgressionBaseValueInSeconds
            };

            return TimeSpan.FromSeconds(value);
        }
    }
}