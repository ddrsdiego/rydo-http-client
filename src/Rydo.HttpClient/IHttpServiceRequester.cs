namespace Rydo.HttpClient
{
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHttpServiceRequester
    {
        /// <summary>
        /// Allowed informing a value that is a token for Bearer authorization.
        /// It is not necessary to inform that the type is Bearer, the same already placed inside the authorization header.
        /// </summary>
        /// <param name="token">Valid token that must be informed, must not be null.</param>
        /// <returns></returns>
        IHttpServiceRequester WithBearerToken(string token);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        IHttpServiceRequester WithCorrelationId(string correlationId);

        IHttpServiceRequester WithHeader(string name, string value);

        IHttpServiceRequester WithParameters(params object[] parameters);

        Task<HttpServiceResponse<TResponse>> GetAsync<TResponse>(CancellationToken cancellationToken = default);

        Task<HttpServiceResponse<TResponse>> PostAsync<TContent, TResponse>(TContent content,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PostAsync<TContent>(TContent content, CancellationToken cancellationToken = default);

        Task<HttpServiceResponse<TResponse>> DeleteAsync<TResponse>(
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> DeleteAsync(CancellationToken cancellationToken = default);

        Task<HttpServiceResponse<TResponse>> PutAsync<TContent, TResponse>(TContent content,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PutAsync<TContent>(TContent content,
            CancellationToken cancellationToken = default);

        Task<HttpServiceResponse<TResponse>> PatchAsync<TContent, TResponse>(TContent content,
            CancellationToken cancellationToken = default);

        Task<string> GetStringAsync();

        Task<byte[]> GetByteArrayAsync();

        Task<Stream> GetStreamAsync();
    }
}