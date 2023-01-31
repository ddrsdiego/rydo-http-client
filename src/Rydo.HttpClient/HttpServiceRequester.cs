namespace Rydo.HttpClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Configurations;
    using Formatters;
    using Logging;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Serialization;

    public sealed class HttpServiceRequester : HttpRequestMessage, IHttpServiceRequester
    {
        private const string ErrorDeserializerResponse = "ERROR_DESERIALIZER_RESPONSE";

        private Uri? _uri;
        private string _correlationId;

        private readonly ILogger<HttpServiceRequester> _logger;
        // private readonly IMemoryCache _memoryCache;

        internal HttpServiceRequester(HttpClient httpClient, EndpointEntry endpointEntry, ISerializer serializer,
            ILogger<HttpServiceRequester> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            EndpointEntry = endpointEntry ?? throw new ArgumentNullException(nameof(endpointEntry));
            _correlationId = string.Empty;

            // _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        internal static IHttpServiceRequesterBuilder Builder() => new HttpServiceRequesterBuilder();

        private HttpClient Client { get; }

        private EndpointEntry EndpointEntry { get; }

        private ISerializer Serializer { get; }

        internal Uri Uri
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_uri == null)
                    _uri = EndpointEntry.GetEndpointPath();
                return _uri;
            }
            private set => _uri = value;
        }

        private string CorrelationId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (string.IsNullOrEmpty(_correlationId))
                    _correlationId = Guid.NewGuid().ToString().Split('-')[4];
                return _correlationId;
            }
        }

        public IHttpServiceRequester WithParameters(params object[] parameters)
        {
            Helper.GuardNull(parameters, nameof(parameters));

            var endpointPath = EndpointEntry.GetEndpointPath();

            // var endpointEntryName = EndpointEntry.Name;
            // var formattedParameters = string.Join('-', parameters);
            //
            // var key = $"{endpointEntryName}-{formattedParameters}";
            // if (_memoryCache.TryGetValue(key, out Uri newUri))
            // {
            //     Uri = newUri;
            //     return this;
            // }

            var uriFormatted = UriFormatter.Format(endpointPath.OriginalString, parameters);

            Uri = new Uri(uriFormatted);

            // _memoryCache.Set(key, Uri);

            return this;
        }

        public IHttpServiceRequester WithCorrelationId(string correlationId)
        {
            Helper.GuardNull(correlationId);

            _correlationId = correlationId;
            return this;
        }

        public IHttpServiceRequester WithHeader(string name, string value)
        {
            Helper.GuardNull(name, nameof(name));
            Helper.GuardNull(value, nameof(value));

            Headers.TryAddWithoutValidation(name, value);
            return this;
        }

        public IHttpServiceRequester WithBearerToken(string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            const string bearerToken = "Bearer";

            Headers.Authorization = new AuthenticationHeaderValue(bearerToken, token);
            return this;
        }

        public async Task<HttpServiceResponse<TResponse>> GetAsync<TResponse>(
            CancellationToken cancellationToken = default) =>
            await CreateResultAsync<TResponse>(await SendAsync(HttpMethod.Get, cancellationToken));

        public async Task<HttpServiceResponse<TResponse>> PostAsync<TContent, TResponse>(TContent content,
            CancellationToken cancellationToken = default)
        {
            SetContent(content);
            return await CreateResultAsync<TResponse>(await SendAsync(HttpMethod.Post, cancellationToken));
        }

        public async Task<HttpResponseMessage> PostAsync<TContent>(TContent content,
            CancellationToken cancellationToken = default)
        {
            SetContent(content);
            return await SendAsync(HttpMethod.Post, cancellationToken);
        }

        public async Task<HttpServiceResponse<TResponse>> DeleteAsync<TResponse>(
            CancellationToken cancellationToken = default) =>
            await CreateResultAsync<TResponse>(await SendAsync(HttpMethod.Delete, cancellationToken));

        public async Task<HttpResponseMessage> DeleteAsync(CancellationToken cancellationToken = default) =>
            await SendAsync(HttpMethod.Delete, cancellationToken);

        public async Task<HttpServiceResponse<TResponse>> PutAsync<TContent, TResponse>(TContent content,
            CancellationToken cancellationToken = default)
        {
            SetContent(content);
            return await CreateResultAsync<TResponse>(await SendAsync(HttpMethod.Put, cancellationToken));
        }

        public async Task<HttpResponseMessage> PutAsync<TContent>(TContent content,
            CancellationToken cancellationToken = default)
        {
            SetContent(content);
            return await SendAsync(HttpMethod.Put, cancellationToken);
        }

        public async Task<HttpServiceResponse<TResponse>> PatchAsync<TContent, TResponse>(TContent content,
            CancellationToken cancellationToken) =>
            await CreateResultAsync<TResponse>(await SendAsync(HttpMethod.Patch, cancellationToken));

        public async Task<HttpResponseMessage> PatchAsync<TContent>(TContent content, params object[] parameters) =>
            await Task.FromResult(new HttpResponseMessage());

        public async Task<string> GetStringAsync() => await Client.GetStringAsync(Uri);

        public async Task<byte[]> GetByteArrayAsync() => await Client.GetByteArrayAsync(Uri);

        public async Task<Stream> GetStreamAsync() => await Client.GetStreamAsync(Uri);

        private void SetContent<TContent>(TContent content) =>
            Content = new StringContent(Serializer.Serialize(content), Encoding.UTF8, MediaTypeNames.Application.Json);

        private static bool TryGetCorrelationIdFromRequest<T>(HttpResponseMessage responseMessage,
            out string? correlationId)
        {
            correlationId = string.Empty;
            IEnumerable<string>? values = null;

            responseMessage.RequestMessage?.Headers.TryGetValues(Constants.XCorrelationId, out values);
            if (values != null && !values.Any())
                return false;

            correlationId = values?.FirstOrDefault();

            return true;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod,
            CancellationToken cancellationToken = default)
        {
            Method = httpMethod;
            Client.DefaultRequestHeaders.Add(Constants.XCorrelationId, CorrelationId);

            var context = new Context
            {
                {Constants.PolicyContextLogger, _logger},
                {Constants.XCorrelationId, CorrelationId},
                {Constants.EndpointEntryName, EndpointEntry.Name}
            };

            this.SetPolicyExecutionContext(context);

            RequestUri = Uri;
            return await Client.SendAsync(this, cancellationToken);
        }

        private async Task<HttpServiceResponse<T>> CreateResultAsync<T>(HttpResponseMessage responseMessage)
        {
            TryGetCorrelationIdFromRequest<T>(responseMessage, out var correlationId);

            if (!responseMessage.IsValidToDeserialize())
                return HttpServiceResponse<T>.Create(responseMessage, correlationId, default);

            try
            {
                var contentAsJson = await responseMessage.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(contentAsJson))
                    return HttpServiceResponse<T>.Create(responseMessage, correlationId, default);

                var value = Serializer.Deserialize<T>(contentAsJson);
                return HttpServiceResponse<T>.Create(responseMessage, correlationId, value);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(ex,
                        $"{RydoHttpLogFields.LogType} - Unable to deserialize the response, StatusCode: {RydoHttpLogFields.StatusCode} - cid: {RydoHttpLogFields.CorrelationId} - Endpoint: {RydoHttpLogFields.EndpointName}",
                        ErrorDeserializerResponse,
                        responseMessage.StatusCode,
                        correlationId,
                        EndpointEntry.Name);
                }

                return HttpServiceResponse<T>.Create(responseMessage, correlationId, default);
            }
        }
    }
}