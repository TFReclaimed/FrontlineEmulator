using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Utils;

public class JsonStringConverter<T> : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonString = reader.GetString();
        if (jsonString is null)
        {
            return default;
        }
        
        return JsonSerializer.Deserialize<T>(jsonString, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var jsonString = JsonSerializer.Serialize(value, options);
        writer.WriteStringValue(jsonString);
    }
}