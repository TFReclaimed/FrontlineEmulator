using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CombatBuffsCCGEvent : CCGEventData
{
    public CCGEventType BuffType { get; }

    [JsonPropertyName("attackerCardID")]
    public int AttackerCardId { get; }

    public sbyte AttackCardOwner { get; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; }

    public sbyte TargetCardOwner { get; }

    public EventLogTraitCardInfo[] BuffTraits { get; set; }

    public CombatBuffsCCGEvent()
    {
    }

    public CombatBuffsCCGEvent(CCGEventType type, int attackerID, sbyte attackerOwner, int targetID, sbyte targetOwner)
    {
        BuffType = type;
        AttackerCardId = attackerID;
        AttackCardOwner = attackerOwner;
        TargetCardId = targetID;
        TargetCardOwner = targetOwner;
    }

    public override CCGEventType Type()
    {
        return BuffType;
    }
}