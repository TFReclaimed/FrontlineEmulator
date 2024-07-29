using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class ConditionalOperatorConverter : JsonConverter<Operator>
{
    public override Operator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "=" => Operator.IsEqual,
            "!=" => Operator.IsNotEqual,
            "<" => Operator.IsLessThan,
            "<=" => Operator.IsLessThanOrEqual,
            ">" => Operator.IsGreaterThan,
            ">=" => Operator.IsGreaterThanOrEqual,
            _ => Operator.Invalid
        };
    }

    public override void Write(Utf8JsonWriter writer, Operator value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            Operator.IsEqual => "=",
            Operator.IsNotEqual => "!=",
            Operator.IsLessThan => "<",
            Operator.IsLessThanOrEqual => "<=",
            Operator.IsGreaterThan => ">",
            Operator.IsGreaterThanOrEqual => ">=",
            _ => ""
        });
    }
}