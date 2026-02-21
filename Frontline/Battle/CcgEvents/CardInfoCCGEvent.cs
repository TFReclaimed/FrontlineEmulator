namespace Frontline.Battle.CcgEvents;

public class CardInfoCCGEvent : CCGEventData
{
    public CCGEventType eventType;

    public int instanceId;

    public int data;

    public sbyte owner;

    public string info;

    public CardInfoCCGEvent()
    {
    }

    public CardInfoCCGEvent(CCGEventType type, int cardID, sbyte cardOwner, int value, string eventInfo)
    {
        eventType = type;
        instanceId = cardID;
        owner = cardOwner;
        data = value;
        info = eventInfo;
    }

    public override CCGEventType Type()
    {
        return eventType;
    }
}