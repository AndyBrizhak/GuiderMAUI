using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuiderMAUI.Shared.Utils 
{
    public class DescriptionConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.StartArray:
                    var list = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                    if (list != null)
                        return string.Join("", list.Select(s => $"<p>{s}</p>"));
                    return string.Empty;
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new JsonException($"Unexpected token type {reader.TokenType} for Description.");
            }
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}