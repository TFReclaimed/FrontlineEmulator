using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter<Region>))]
public enum Region
{
    Player0,
    Player1,
    Control,
    NumRegions
}