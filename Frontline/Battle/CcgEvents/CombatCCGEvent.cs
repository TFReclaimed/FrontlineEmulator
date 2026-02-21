using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CombatCCGEvent : CCGEventData
{
    public CCGEventType CombatType { get; set; }

    [JsonPropertyName("attackerCardID")]
    public int AttackerCardId { get; set; }

    public sbyte AttackCardOwner { get; set; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; set; }

    public sbyte TargetCardOwner { get; set; }

    public sbyte AttackTotal { get; set; }

    public sbyte BypassTotal { get; set; }

    public sbyte Result { get; set; }

    public CombatCCGEvent()
    {
    }

    public CombatCCGEvent(CCGEventType type, int attackerID, sbyte attackOwner, int targetID, sbyte targetOwner,
        sbyte attack, sbyte bypass)
    {
        CombatType = type;
        AttackerCardId = attackerID;
        TargetCardId = targetID;
        AttackCardOwner = attackOwner;
        TargetCardOwner = targetOwner;
        AttackTotal = attack;
        BypassTotal = bypass;
    }

    public override CCGEventType Type()
    {
        return CombatType;
    }
}