using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TraitDurationType
{
    Instant,
    Permanent,
    EndOfTurn,
    EndOfMyTurn,
    EndOfEnemyTurn,
    StartOfTurn,
    StartOfMyTurn,
    StartOfEnemyTurn,
    NumDurations
}