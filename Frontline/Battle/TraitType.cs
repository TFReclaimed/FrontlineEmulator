using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TraitType : byte
{
    Basic = 0,
    Passive = 1,
    Deployed = 2,
    OneShot = 3,
    Assault = 4,
    LastStand = 5,
    Secret = 6,
    BurnCard = 7,
    Gear = 8,
    Upgrade = 9
}