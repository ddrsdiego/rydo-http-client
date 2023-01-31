namespace Rydo.HttpClient.Serialization
{
    using System.Linq;
    using System.Text.Json;

    internal sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) =>
            ToSnakeCase(name);

        private static string ToSnakeCase(string value) =>
            string.Concat(
                value.Select(
                    (c, i) => i > 0 && char.IsUpper(c)
                        ? "_" + c
                        : c.ToString()
                )
            ).ToLower();
    }
}