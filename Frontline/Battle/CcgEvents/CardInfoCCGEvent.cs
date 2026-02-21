namespace Frontline.Battle.CcgEvents;

public class CardInfoCCGEvent : CCGEventData
{
    public CCGEventType EventType { get; set; }

    public int InstanceId { get; set; }

    public int Data { get; set; }

    public sbyte Owner { get; set; }

    public string Info { get; set; }

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