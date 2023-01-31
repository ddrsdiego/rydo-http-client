namespace Rydo.HttpClient
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public interface IHttpServiceRequesterFactory
    {
        /// <summary>
        ///  Creates an instance of the IHttpServiceRequester interface, configured with the parameters provided in the appSettings file.
        /// </summary>
        /// <param name="endpointName">Value must match the value configured in the Clients.Endpoints.Name tag.</param>
        /// <returns>A singleton instance of HttpServiceRequester</returns>
        public IHttpServiceRequester CreateRequestFor(string endpointName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        IHttpServiceRequesterFactory SetCustomConvert(JsonConverter converter);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="converters"></param>
        /// <returns></returns>
        IHttpServiceRequesterFactory SetCustomConvert(IEnumerable<JsonConverter> converters);
    }
}