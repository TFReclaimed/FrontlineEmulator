namespace Frontline.Battle;

public enum TargetTypeMod : byte
{
    HasIntercept = 0,
    HasStealth = 1,
    NotInstallation = 2,
    Piloted = 3,
    NotPiloted = 4,
    EmbarkedPilot = 5,
    IsWounded = 6,
    IsStunned = 7,
    IsDetered = 8,
    IsImmobalized = 9,
    IsActive = 10,
    NotActive = 11,
    HasAttack = 12,
    NumMods = 13
}