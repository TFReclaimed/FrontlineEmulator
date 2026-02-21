namespace Frontline.Battle.CcgEvents;

public class CardTraumaCCGEvent : CCGEventData
{
    public CCGEventType traumaType;

    public int health;

    public int sourceCardID;

    public int targetCardID;

    public sbyte sourceOwner;

    public sbyte targetOwner;

    public CardTraumaCCGEvent()
    {
    }

    public CardTraumaCCGEvent(CCGEventType type, int healthDelta, int sourceId, sbyte sourceCardOwner, int targetId,
        sbyte targetCardOwner)
    {
        traumaType = type;
        health = healthDelta;
        sourceCardID = sourceId;
        targetCardID = targetId;
        sourceOwner = sourceCardOwner;
        targetOwner = targetCardOwner;
    }

    public override CCGEventType Type()
    {
        return traumaType;
    }
}