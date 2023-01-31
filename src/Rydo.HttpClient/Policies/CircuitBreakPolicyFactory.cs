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

    internal static class CircuitBreakPolicyFactory
    {
        private const string PolicyType = "circuit-break";
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakPolicy(this EndpointEntry endpointEntry)
        {
            Helper.GuardNull(endpointEntry, nameof(endpointEntry));

            var policy = endpointEntry.CircuitBreak is null
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
                .CircuitBreakerAsync(endpointEntry.CircuitBreak!.EventsTimesBeforeBreaking,
                    TimeSpan.FromSeconds(endpointEntry.CircuitBreak.DurationOfBreakInSeconds),
                    onBreak: OnBreak(policyKey),
                    onReset: OnReset(policyKey)).WithPolicyKey(policyKey);
        }

        private static Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context> OnBreak(string policyKey) =>
            (outcome, timespan, context) => RegisterLogForOnBreak(context, policyKey, timespan);

        private static void RegisterLogForOnBreak(Context context, string policyKey, TimeSpan timespan)
        {
            var correlationId = context[Constants.XCorrelationId].ToString();
            var endpointName = context[Constants.EndpointEntryName].ToString();
            var logger = context[Constants.PolicyContextLogger] as ILogger<HttpServiceRequester>;

            logger?.LogWarning(EventIds.RequestOpenedCircuit,
                $"{RydoHttpLogFields.LogType} {RydoHttpLogFields.EndpointName} {RydoHttpLogFields.CorrelationId} Opening the circuit for {RydoHttpLogFields.WaitingTimeSpan} seconds to Policy Key {RydoHttpLogFields.PolicyKey}",
                RydoHttpLogTypes.HttpRequestOut,
                endpointName,
                correlationId,
                timespan.TotalSeconds,
                policyKey);
        }

        private static Action<Context> OnReset(string policyKey)
        {
            return context =>
            {
                var correlationId = context[Constants.XCorrelationId].ToString();
                var endpointName = context[Constants.EndpointEntryName].ToString();
                var logger = context[Constants.PolicyContextLogger] as ILogger<HttpServiceRequester>;

                logger?.LogWarning(EventIds.RequestClosedCircuit,
                    $"{RydoHttpLogFields.LogType} {RydoHttpLogFields.EndpointName} {RydoHttpLogFields.CorrelationId} Closing the circuit to Policy Key {RydoHttpLogFields.PolicyKey}",
                    RydoHttpLogTypes.HttpRequestOut,
                    endpointName,
                    correlationId,
                    policyKey);
            };
        }
    }
}