using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class StringFloatConverter : JsonConverter<float>
{
    private readonly float _defaultValue;

    public StringFloatConverter(float defaultValue = 0f)
    {
        _defaultValue = defaultValue;
    }

    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue is null || string.IsNullOrEmpty(stringValue))
            {
                return _defaultValue;
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

[AttributeUsage(AttributeTargets.Property)]
public class StringFloatConverterAttribute : JsonConverterAttribute
{
    private readonly float _defaultValue;

    public StringFloatConverterAttribute(float defaultValue)
    {
        _defaultValue = defaultValue;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new StringFloatConverter(_defaultValue);
    }
}