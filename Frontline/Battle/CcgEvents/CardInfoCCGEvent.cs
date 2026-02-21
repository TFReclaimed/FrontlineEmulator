namespace Frontline.Battle.CcgEvents;

public class CardInfoCCGEvent : CCGEventData
{
    public CCGEventType EventType { get; }

    public int InstanceId { get; }

    public int Data { get; }

    public sbyte Owner { get; }

    public string Info { get; }

    public CardInfoCCGEvent()
    {
    }

    public CardInfoCCGEvent(CCGEventType type, int cardID, sbyte cardOwner, int value, string eventInfo)
    {
        EventType = type;
        InstanceId = cardID;
        Owner = cardOwner;
        Data = value;
        Info = eventInfo;
    }

    public override CCGEventType Type()
    {
        return EventType;
    }
}