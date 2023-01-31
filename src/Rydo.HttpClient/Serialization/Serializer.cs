namespace Rydo.HttpClient.Serialization
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public abstract class Serializer : ISerializer
    {
        protected readonly JsonSerializerOptions? Options = new JsonSerializerOptions
        {
            
        };

        protected Serializer(IEnumerable<JsonConverter>? converters = null)
        {
            if (converters == null) return;

            foreach (var converter in converters)
                Options.Converters.Add(converter);
        }

        public abstract string Serialize<T>(T source);

        public abstract T Deserialize<T>(string json);
    }
}