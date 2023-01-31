namespace Rydo.HttpClient.UnitTest.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class ListToListKeyValuePairJsonConverter : JsonConverter<object>
    {
        public ListToListKeyValuePairJsonConverter() { }
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.FullName.StartsWith("System.Collections.Generic.List`1[[System.Collections.Generic.KeyValuePair`2"))
                return true;

            return false;
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                return null;
            }

            reader.Read();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                return null;
            }

            List<KeyValuePair<string?, object?>> listValues = new List<KeyValuePair<string?, object?>>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString().ToLower() == "key") {
                    string key;
                    string? value;

                    reader.Read();

                    key = reader.GetString();

                    reader.Read();
                    reader.Read();

                    value = reader.GetString();

                    var current = new KeyValuePair<string?, object?>(key, value);
                    listValues.Add(current);
                }

                if (reader.TokenType == JsonTokenType.EndArray)
                    return listValues;
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            var str = value.ToString();
            if (int.TryParse(str, out var i))
            {
                writer.WriteNumberValue(i);
            }
            else if (double.TryParse(str, out var d))
            {
                writer.WriteNumberValue(d);
            }
            else
            {
                throw new Exception($"unable to parse {str} to number");
            }
        }
    }
}