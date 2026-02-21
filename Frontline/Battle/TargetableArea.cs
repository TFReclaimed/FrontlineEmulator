using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetableArea : byte
{
    Self = 0,
    UnitStack = 1,
    CurrentRegion = 2,
    AnyRegion = 3,
    AnyCommander = 4,
    FriendlyPerimeter = 5,
    EnemyPerimeter = 6,
    Frontline = 7,
    FriendlyRegions = 8,
    EnemyRegions = 9,
    FriendlyCommander = 10,
    EnemyCommander = 11,
    BattleField = 12,
    BattleFieldNC = 13,
    FriendlyHand = 14,
    EnemyHand = 15,
    FriendlyDiscard = 16,
    EnemyDiscard = 17,
    AnyAreas = 18
}