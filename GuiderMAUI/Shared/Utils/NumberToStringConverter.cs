using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuiderMAUI.Shared.Utils
{
    public class NumberToStringConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long longValue)) return longValue.ToString();
                    if (reader.TryGetDouble(out double doubleValue)) return doubleValue.ToString();
                    return null;
                case JsonTokenType.Null:
                    return null;
                default:
                    return null;
            }
        }
        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
