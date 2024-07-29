using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class ConditionalConjunctionConverter : JsonConverter<Conjunction>
{
    public override Conjunction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "AND" => Conjunction.And,
            "OR" => Conjunction.Or,
            _ => Conjunction.None
        };
    }

    public override void Write(Utf8JsonWriter writer, Conjunction value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            Conjunction.And => "AND",
            Conjunction.Or => "OR",
            _ => ""
        });
    }
}