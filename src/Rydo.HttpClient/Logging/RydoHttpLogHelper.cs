namespace Rydo.HttpClient.Logging
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;

    internal static class RydoHttpLogHelper
    {
        private static readonly Func<ILogger, HttpMethod, Uri, string, IDisposable> _beginRequestPipelineScope =
            LoggerMessage.DefineScope<HttpMethod, Uri, string>(
                "HTTP {HttpMethod} {Uri} {CorrelationId}");

        /// <summary>
        /// 
        /// </summary>
        private static readonly Action<ILogger, string, string, string, HttpMethod, Uri, Exception?>
            _requestPipelineStart =
                LoggerMessage.Define<string, string, string, HttpMethod, Uri>(
                    LogLevel.Debug,
                    EventIds.RequestStarted,
                    $"{RydoHttpLogFields.LogType} {RydoHttpLogFields.EndpointName} {RydoHttpLogFields.CorrelationId} Start processing HTTP request {RydoHttpLogFields.HttpMethod} {RydoHttpLogFields.Uri}"
                );

        private static readonly
            Action<ILogger, string?, string?, string?, HttpMethod?, Uri?, long?, Exception?>
            _requestPipelineEnd =
                LoggerMessage.Define<string?, string?, string?, HttpMethod?, Uri?, long?>(
                    LogLevel.Information,
                    EventIds.RequestEnded,
                    $"{RydoHttpLogFields.LogType} {RydoHttpLogFields.EndpointName} {RydoHttpLogFields.CorrelationId} End processing HTTP request {RydoHttpLogFields.HttpMethod} {RydoHttpLogFields.Uri} after {RydoHttpLogFields.ElapsedMilliseconds}ms");

        public static IDisposable BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request)
        {
            var correlationId = GetCorrelationIdFromRequest(request);
            return _beginRequestPipelineScope(logger, request.Method, request.RequestUri!, correlationId);
        }

        public static void RequestPipelineStart(ILogger logger, string endpointName, HttpRequestMessage request)
        {
            var correlationId = GetCorrelationIdFromRequest(request);

            _requestPipelineStart(logger, RydoHttpLogTypes.HttpRequestOut, endpointName, correlationId, request.Method,
                request.RequestUri!, null);
        }

        public static void RequestPipelineEnd(ILogger logger, string endpointName, HttpResponseMessage response,
            long elapsedMilliseconds)
        {
            var correlationId = GetCorrelationIdFromRequest(response.RequestMessage);

            _requestPipelineEnd(logger, RydoHttpLogTypes.HttpRequestOut, endpointName, correlationId,
                response?.RequestMessage?.Method, response?.RequestMessage?.RequestUri, elapsedMilliseconds, null);
        }

        private static string GetCorrelationIdFromRequest(HttpRequestMessage? request)
        {
            var correlationId = "Not set";

            if (request is null)
                return correlationId;

            if (request.Headers.TryGetValues(Constants.XCorrelationId, out var values))
            {
                correlationId = values.First();
            }

            return correlationId;
        }
    }
}