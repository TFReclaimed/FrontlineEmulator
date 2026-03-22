using System.Text.Json;
using System.Text.Json.Serialization;
using Frontline.Battle.Data.Card;

namespace Frontline.Missions.Json;

public class FactionConverter : JsonConverter<CardFaction>
{
    public override CardFaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "IMC" => CardFaction.Imc,
            "MIL" => CardFaction.Militia,
            "NEU" => CardFaction.Neutral,
            _ => CardFaction.Neutral
        };
    }

    public override void Write(Utf8JsonWriter writer, CardFaction value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            CardFaction.Imc => "IMC",
            CardFaction.Militia => "MIL",
            CardFaction.Neutral => "NEU",
            _ => "NEU"
        });
    }
}