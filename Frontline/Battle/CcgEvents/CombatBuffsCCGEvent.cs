using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CombatBuffsCCGEvent : CCGEventData
{
    public CcgEventType BuffType { get; set; }

    [JsonPropertyName("attackerCardID")]
    public int AttackerCardId { get; set; }

    public sbyte AttackCardOwner { get; set; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; set; }

    public sbyte TargetCardOwner { get; set; }

    public EventLogTraitCardInfo[] BuffTraits { get; set; }

    public CombatBuffsCCGEvent()
    {
    }

    public CombatBuffsCCGEvent(CcgEventType type, int attackerID, sbyte attackerOwner, int targetID, sbyte targetOwner)
    {
        BuffType = type;
        AttackerCardId = attackerID;
        AttackCardOwner = attackerOwner;
        TargetCardId = targetID;
        TargetCardOwner = targetOwner;
    }

    public override CcgEventType Type()
    {
        return BuffType;
    }
}