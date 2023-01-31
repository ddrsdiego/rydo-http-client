namespace Rydo.HttpClient.Serialization
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal sealed class DefaultSerializer : Serializer
    {
        public DefaultSerializer(IEnumerable<JsonConverter>? converters = null)
            : base(converters)
        {
        }

        public override string Serialize<T>(T source) => JsonSerializer.Serialize(source);

        public override T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options)!;
    }
}