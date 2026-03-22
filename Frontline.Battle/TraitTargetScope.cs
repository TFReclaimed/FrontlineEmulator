using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter<TraitTargetScope>))]
public enum TraitTargetScope
{
    Self,
    UnitStack,
    FriendlyUnit,
    FriendlyUnitNotSelf,
    EnemyUnit,
    AllFriendly,
    AllFriendlyNotSelf,
    AllEnemy,
    TriggeringUnit,
    TriggerTarget,
    RandomFriendly,
    RandomFriendlyNotSelf,
    RandomEnemy,
    AnyScope
}