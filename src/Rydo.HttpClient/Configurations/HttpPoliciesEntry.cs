namespace Rydo.HttpClient.Configurations
{
    using System.Collections.Generic;

    public sealed class HttpPoliciesEntry
    {
        public HttpPoliciesEntry()
        {
            Entries = new Dictionary<string, HttpPoliciesItem>();
        }

        public IDictionary<string, HttpPoliciesItem> Entries { get; set; }
    }
}