using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CombatCcgEvent : CcgEventData
{
    public CcgEventType CombatType { get; set; }

    [JsonPropertyName("attackerCardID")]
    public int AttackerCardId { get; set; }

    public sbyte AttackCardOwner { get; set; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; set; }

    public sbyte TargetCardOwner { get; set; }

    public sbyte AttackTotal { get; set; }

    public sbyte BypassTotal { get; set; }

    public sbyte Result { get; set; }

    public CombatCcgEvent(CcgEventType type, int attackerId, sbyte attackOwner, int targetId, sbyte targetOwner,
        sbyte attack, sbyte bypass)
    {
        CombatType = type;
        AttackerCardId = attackerId;
        TargetCardId = targetId;
        AttackCardOwner = attackOwner;
        TargetCardOwner = targetOwner;
        AttackTotal = attack;
        BypassTotal = bypass;
    }
}