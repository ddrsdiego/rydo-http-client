namespace Rydo.HttpClient.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    internal static class SerializerFactory
    {
        private const string Snake = nameof(Snake);

        public static ISerializer Create(string? type, IEnumerable<JsonConverter>? converters = null)
        {
            if (string.IsNullOrWhiteSpace(type))
                return new DefaultSerializer(converters);

            if (string.CompareOrdinal(type, Snake) == 0)
                return new SnakeCaseSerializer(converters);

            throw new ArgumentException(CreateExceptionMessage(type));
        }

        internal static string CreateExceptionMessage(string typeName) => 
            $"Invalid serializer type: {typeName}";
    }
}