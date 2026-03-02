using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetableArea
{
    Self,
    UnitStack,
    CurrentRegion,
    AnyRegion,
    AnyCommander,
    FriendlyPerimeter,
    EnemyPerimeter,
    Frontline,
    FriendlyRegions,
    EnemyRegions,
    FriendlyCommander,
    EnemyCommander,
    BattleField,
    [JsonStringEnumMemberName("BattleFieldNC")]
    BattleFieldNc,
    FriendlyHand,
    EnemyHand,
    FriendlyDiscard,
    EnemyDiscard,
    AnyAreas
}