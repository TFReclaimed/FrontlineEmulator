using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontline.Missions.Json;

public class PveRegionConverter : JsonConverter<PveRegion>
{
    public override PveRegion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null or "All")
        {
            return PveRegion.NumRegions;
        }
        
        return (PveRegion) Enum.Parse(typeof(PveRegion), value);
    }

    public override void Write(Utf8JsonWriter writer, PveRegion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value == PveRegion.NumRegions ? "All" : value.ToString());
    }
}