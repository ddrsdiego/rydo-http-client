namespace Rydo.HttpClient.Serialization
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal sealed class SnakeCaseSerializer : Serializer
    {
        public SnakeCaseSerializer(IEnumerable<JsonConverter>? converters = null)
            : base(converters)
        {
            Options.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
        }

        public override string Serialize<T>(T source) => JsonSerializer.Serialize(source, Options);

        public override T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
    }
}