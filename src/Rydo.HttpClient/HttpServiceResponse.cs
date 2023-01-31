namespace Rydo.HttpClient
{
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// a
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct HttpServiceResponse<T>
    {
        private HttpServiceResponse(HttpResponseMessage responseMessage, string correlationId, T result) =>
            (ResponseMessage, CorrelationId, Result) = (responseMessage, correlationId, result);

        public T Result { get; }

        private HttpResponseMessage ResponseMessage { get; }
        
        public string CorrelationId { get; }

        /// <summary>
        /// Gets a value that indicates if the HTTP response was successful.
        /// </summary>
        /// <returns>true if StatusCode was in the range 200-299; otherwise, false.</returns>
        public bool IsSuccessStatusCode => ResponseMessage.IsSuccessStatusCode;

        /// <summary>
        /// Gets or sets the status code of the HTTP response.
        /// </summary>
        /// <returns>The status code of the HTTP response.</returns>
        public HttpStatusCode StatusCode => ResponseMessage.StatusCode;
        
        /// <summary>
        /// Gets or sets the content of a HTTP response message
        /// </summary>
        /// <returns>The content of the HTTP response message.</returns>
        public HttpContent Content => ResponseMessage.Content;
        
        internal static HttpServiceResponse<T> Create(HttpResponseMessage responseMessage, string correlationId,
            T result) => new HttpServiceResponse<T>(responseMessage, correlationId, result);
    }
}