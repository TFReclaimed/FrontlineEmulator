using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class StringFloatConverter : JsonConverter<float>
{
    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue is null || string.IsNullOrEmpty(stringValue))
            {
                return 0;
            }

            return float.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        throw new JsonException($"Unexpected token parsing float. Expected String, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("F", CultureInfo.InvariantCulture));
    }
}