using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CombatBuffsCcgEvent : CcgEventData
{
    public CcgEventType BuffType { get; set; }

    [JsonPropertyName("attackerCardID")]
    public int AttackerCardId { get; set; }

    public sbyte AttackCardOwner { get; set; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; set; }

    public sbyte TargetCardOwner { get; set; }

    public EventLogTraitCardInfo[] BuffTraits { get; set; }

    public CombatBuffsCcgEvent(CcgEventType type, int attackerId, sbyte attackerOwner, int targetId, sbyte targetOwner)
    {
        BuffType = type;
        AttackerCardId = attackerId;
        AttackCardOwner = attackerOwner;
        TargetCardId = targetId;
        TargetCardOwner = targetOwner;
    }
}