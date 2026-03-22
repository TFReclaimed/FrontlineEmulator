using System.Text.Json;
using System.Text.Json.Serialization;
using Frontline.Battle.Data.Card;

namespace Frontline.Missions.Json;

public class CardRarityConverter : JsonConverter<CardRarity>
{
    public override CardRarity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return CardRarity.NumRarities;
        }
        
        return (CardRarity) Enum.Parse(typeof(CardRarity), value);
    }

    public override void Write(Utf8JsonWriter writer, CardRarity value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}