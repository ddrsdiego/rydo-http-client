namespace Rydo.HttpClient.Logging
{
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    internal sealed class CustomLoggingScopeHttpMessageHandler : DelegatingHandler
    {
        private string _endpointName;
        private readonly ILogger _logger;

        public CustomLoggingScopeHttpMessageHandler(ILogger logger)
        {
            Helper.GuardNull(logger, nameof(logger));

            _logger = logger;
            _endpointName = string.Empty;
        }

        public CustomLoggingScopeHttpMessageHandler(ILogger logger, string endpointName)
        {
            Helper.GuardNull(logger, nameof(logger));
            Helper.GuardNull(endpointName, nameof(endpointName));

            _logger = logger;
            _endpointName = endpointName;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Helper.GuardNull(request, nameof(request));

            using (RydoHttpLogHelper.BeginRequestPipelineScope(_logger, request))
            {
                var stopwatch = Stopwatch.StartNew();

                RydoHttpLogHelper.RequestPipelineStart(_logger, _endpointName, request);

                var response = await base.SendAsync(request, cancellationToken);

                RydoHttpLogHelper.RequestPipelineEnd(_logger, _endpointName, response, stopwatch.ElapsedMilliseconds);

                return response;
            }
        }
    }
}