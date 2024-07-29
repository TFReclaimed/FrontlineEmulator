using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class FactionConverter : JsonConverter<Faction>
{
    public override Faction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "IMC" => Faction.IMC,
            "MIL" => Faction.Militia,
            "NEU" => Faction.Neutral,
            _ => Faction.Neutral
        };
    }

    public override void Write(Utf8JsonWriter writer, Faction value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            Faction.IMC => "IMC",
            Faction.Militia => "MIL",
            Faction.Neutral => "NEU",
            _ => "NEU"
        });
    }
}