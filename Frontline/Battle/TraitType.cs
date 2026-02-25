using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TraitType
{
    Basic,
    Passive,
    Deployed,
    OneShot,
    Assault,
    LastStand,
    Secret,
    BurnCard,
    Gear,
    Upgrade
}