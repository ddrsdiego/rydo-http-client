namespace Rydo.HttpClient
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    internal class HttpPolicyConfiguration
    {
        public HttpPolicyConfiguration()
        {
            Policies = new Dictionary<string, PolicyConfiguration>();
        }

        public IDictionary<string, PolicyConfiguration> Policies { get; internal set; }
    }
    
    internal static class HttpResponseMessageEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public static bool IsValidToDeserialize(this HttpResponseMessage responseMessage) =>
            ResponseHasValidaStatusCode(responseMessage) 
            && ContentHasValue(responseMessage);

        private static bool ResponseHasValidaStatusCode(HttpResponseMessage responseMessage)
        {
            var validStatusCode = responseMessage.StatusCode == HttpStatusCode.OK
                || responseMessage.StatusCode == HttpStatusCode.Created
                || responseMessage.StatusCode == HttpStatusCode.Accepted;

            return validStatusCode;
        }

        private static bool ContentHasValue(HttpResponseMessage responseMessage) => responseMessage.Content != null;
    }
}