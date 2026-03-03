using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter<TargetTypeMod>))]
public enum TargetTypeMod
{
    HasIntercept,
    HasStealth,
    NotInstallation,
    Piloted,
    NotPiloted,
    EmbarkedPilot,
    IsWounded,
    IsStunned,
    [JsonStringEnumMemberName("IsDetered")]
    IsDeterred,
    [JsonStringEnumMemberName("IsImmobalized")]
    IsImmobilized,
    IsActive,
    NotActive,
    HasAttack,
    NumMods
}