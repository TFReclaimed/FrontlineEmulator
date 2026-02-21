namespace Frontline.Battle;

public enum TraitDurationType : byte
{
    Instant = 0,
    Permanent = 1,
    EndOfTurn = 2,
    EndOfMyTurn = 3,
    EndOfEnemyTurn = 4,
    StartOfTurn = 5,
    StartOfMyTurn = 6,
    StartOfEnemyTurn = 7,
    NumDurations = 8
}