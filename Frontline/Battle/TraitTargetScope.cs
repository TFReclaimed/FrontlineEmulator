namespace Frontline.Battle;

public enum TraitTargetScope : byte
{
    Self = 0,
    UnitStack = 1,
    FriendlyUnit = 2,
    FriendlyUnitNotSelf = 3,
    EnemyUnit = 4,
    AllFriendly = 5,
    AllFriendlyNotSelf = 6,
    AllEnemy = 7,
    TriggeringUnit = 8,
    TriggerTarget = 9,
    RandomFriendly = 10,
    RandomFriendlyNotSelf = 11,
    RandomEnemy = 12,
    AnyScope = 13
}