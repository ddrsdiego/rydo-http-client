namespace Rydo.HttpClient
{
    using System;
    using System.Net.Http;
    using Configurations;
    using Microsoft.Extensions.Logging;
    using Serialization;

    internal interface IHttpServiceRequesterBuilder
    {
        IHttpServiceRequesterBuilder HttpClient(HttpClient httpClient);
        
        IHttpServiceRequesterBuilder EndpointEntry(EndpointEntry endpointEntry);
        
        IHttpServiceRequesterBuilder Serializer(ISerializer serializer);
        
        IHttpServiceRequesterBuilder Logger(ILogger<HttpServiceRequester> logger);
        
        HttpServiceRequester Build();
    }

    internal sealed class HttpServiceRequesterBuilder : IHttpServiceRequesterBuilder
    {
        private HttpClient? _httpClient;
        private EndpointEntry? _endpointEntry;
        private ISerializer? _serializer;
        private ILogger<HttpServiceRequester>? _logger;

        public IHttpServiceRequesterBuilder HttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            return this;
        }

        public IHttpServiceRequesterBuilder EndpointEntry(EndpointEntry endpointEntry)
        {
            _endpointEntry = endpointEntry ?? throw new ArgumentNullException(nameof(endpointEntry));
            return this;
        }

        public IHttpServiceRequesterBuilder Serializer(ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            return this;
        }
        
        public IHttpServiceRequesterBuilder Logger(ILogger<HttpServiceRequester> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            return this;
        }
        
        public HttpServiceRequester Build()
        {
            return new HttpServiceRequester(_httpClient, _endpointEntry, _serializer, _logger);
        }
    }
}