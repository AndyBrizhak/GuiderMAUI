using System.Text.Json;
using System.Text.Json.Serialization;
using GuiderMAUI.Shared.Models; 

namespace GuiderMAUI.Shared.Utils
{
    public class OwnerListConverter : JsonConverter<List<Owner>?>
    {
        public override List<Owner>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null: return null;
                case JsonTokenType.StartObject:
                    var single = JsonSerializer.Deserialize<Owner>(ref reader, options);
                    return single != null ? new List<Owner> { single } : null;
                case JsonTokenType.StartArray:
                    return JsonSerializer.Deserialize<List<Owner>>(ref reader, options);
                default:
                    reader.Skip();
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, List<Owner>? value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
