namespace Frontline.Battle.CcgEvents;

public class CombatCCGEvent : CCGEventData
{
    public CCGEventType combatType;

    public int attackerCardID;

    public sbyte attackCardOwner;

    public int targetCardID;

    public sbyte targetCardOwner;

    public sbyte attackTotal;

    public sbyte bypassTotal;

    public sbyte result;

    public CombatCCGEvent()
    {
    }

    public CombatCCGEvent(CCGEventType type, int attackerID, sbyte attackOwner, int targetID, sbyte targetOwner,
        sbyte attack, sbyte bypass)
    {
        combatType = type;
        attackerCardID = attackerID;
        targetCardID = targetID;
        attackCardOwner = attackOwner;
        targetCardOwner = targetOwner;
        attackTotal = attack;
        bypassTotal = bypass;
    }

    public override CCGEventType Type()
    {
        return combatType;
    }
}