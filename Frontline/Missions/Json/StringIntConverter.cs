using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class StringIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue is null || string.IsNullOrWhiteSpace(stringValue))
            {
                return 0;
            }

            return int.Parse(stringValue);
        }

        throw new JsonException($"Unexpected token parsing int. Expected String, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}