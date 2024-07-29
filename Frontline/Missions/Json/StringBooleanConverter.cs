using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class StringBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue is null)
            {
                return false;
            }

            return stringValue.ToUpperInvariant() switch
            {
                "YES" or "TRUE" or "1" or "T" => true,
                "NO" or "FALSE" or "0" or "F" => false,
                _ => false
            };
        }

        throw new JsonException($"Unexpected token parsing boolean. Expected String, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value ? "1" : "0");
    }
}