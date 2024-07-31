using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class StringIntConverter : JsonConverter<int>
{
    private readonly int _defaultValue;

    public StringIntConverter(int defaultValue = 0)
    {
        _defaultValue = defaultValue;
    }

    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue is null || string.IsNullOrWhiteSpace(stringValue))
            {
                return _defaultValue;
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

[AttributeUsage(AttributeTargets.Property)]
public class StringIntConverterAttribute : JsonConverterAttribute
{
    private readonly int _defaultValue;

    public StringIntConverterAttribute(int defaultValue)
    {
        _defaultValue = defaultValue;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new StringIntConverter(_defaultValue);
    }
}