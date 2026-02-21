using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RegionEnum : byte
{
    Player0 = 0,
    Player1 = 1,
    Control = 2,
    NumRegions = 3
}