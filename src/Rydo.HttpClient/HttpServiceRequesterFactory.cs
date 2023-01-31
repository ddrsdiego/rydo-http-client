namespace Rydo.HttpClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Net.Http;
    using System.Text.Json.Serialization;
    using Configurations;
    using Microsoft.Extensions.Logging;
    using Serialization;

    public sealed class HttpServiceRequesterFactory : IHttpServiceRequesterFactory
    {
        private const string EndpointNameNotFound = "ENDPOINT_NAME_NOT_FOUND";

        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpServiceRequesterFactory> _logger;
        private readonly IEndpointEntriesContext _endpointEntriesContext;

        private readonly ConcurrentDictionary<Type, JsonConverter> _converters;
        private ImmutableDictionary<string, ISerializer> _serializers;
        private readonly object _lockObject;

        public HttpServiceRequesterFactory(ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IEndpointEntriesContext endpointEntriesContext)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _endpointEntriesContext = endpointEntriesContext ??
                                      throw new ArgumentNullException(nameof(endpointEntriesContext));

            _serializers = ImmutableDictionary<string, ISerializer>.Empty;
            _logger = loggerFactory.CreateLogger<HttpServiceRequesterFactory>();
            _converters = new ConcurrentDictionary<Type, JsonConverter>();
            _lockObject = new object();
        }

        public IHttpServiceRequester CreateRequestFor(string endpointName)
        {
            try
            {
                if (string.IsNullOrEmpty(endpointName))
                    throw new ArgumentNullException(nameof(endpointName));

                var endpointEntry = TryGetEndpointEntry(endpointName);

                Helper.GuardNull(endpointEntry.Name, nameof(endpointEntry.Name));
                
                var httpServiceRequester = HttpServiceRequester
                    .Builder()
                    .HttpClient(_httpClientFactory.CreateClient(endpointEntry.Name))
                    .EndpointEntry(endpointEntry)
                    .Serializer(TryGetSerializer(endpointEntry))
                    .Logger(_loggerFactory.CreateLogger<HttpServiceRequester>())
                    .Build();

                return httpServiceRequester;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        public IHttpServiceRequesterFactory SetCustomConvert(JsonConverter converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            SetCustomConvert(new List<JsonConverter>(1) {converter});

            return this;
        }

        public IHttpServiceRequesterFactory SetCustomConvert(IEnumerable<JsonConverter> converters)
        {
            foreach (var converter in converters)
            {
                _converters.TryAdd(converter.GetType(), converter);
            }

            return this;
        }

        private EndpointEntry TryGetEndpointEntry(string endpointName)
        {
            if (_endpointEntriesContext.Entries.TryGetValue(endpointName, out var endpointEntry))
                return endpointEntry;

            _logger.LogError(
                $"{LogFields.LogType} - Endpoint {LogFields.EndpointName} not found in settings context, endpoint {LogFields.EndpointName} is configured in appSettings?",
                EndpointNameNotFound,
                endpointName,
                endpointName);

            throw new InvalidOperationException(
                $"Endpoint {endpointName} not found in settings context, endpoint {endpointName} is configured in appSettings?");
        }

        private ISerializer TryGetSerializer(EndpointEntry endpointEntry)
        {
            Helper.GuardNull(endpointEntry.Name, nameof(endpointEntry.Name));

            if (_serializers.TryGetValue(endpointEntry.Name, out var serializer))
                return serializer;

            lock (_lockObject)
            {
                if (_serializers.TryGetValue(endpointEntry.Name, out serializer))
                    return serializer;

                serializer = SerializerFactory.Create(endpointEntry.CaseStyle, _converters.Values);
                _serializers = _serializers.Add(endpointEntry.Name, serializer);
            }

            return serializer;
        }
    }
}