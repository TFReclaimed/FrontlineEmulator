namespace Frontline.Battle.CcgEvents;

public class CombatBuffsCCGEvent : CCGEventData
{
    public CCGEventType buffType;

    public int attackerCardID;

    public sbyte attackCardOwner;

    public int targetCardID;

    public sbyte targetCardOwner;

    public EventLogTraitCardInfo[] buffTraits;

    public CombatBuffsCCGEvent()
    {
    }

    public CombatBuffsCCGEvent(CCGEventType type, int attackerID, sbyte attackerOwner, int targetID, sbyte targetOwner)
    {
        buffType = type;
        attackerCardID = attackerID;
        attackCardOwner = attackerOwner;
        targetCardID = targetID;
        targetCardOwner = targetOwner;
    }

    public override CCGEventType Type()
    {
        return buffType;
    }
}