namespace Frontline.Battle.CcgEvents;

public class CardInfoCcgEvent : CcgEventData
{
    public CcgEventType EventType { get; set; }

    public int InstanceId { get; set; }

    public int Data { get; set; }

    public sbyte Owner { get; set; }

    public string Info { get; set; }

    public CardInfoCcgEvent(CcgEventType type, int cardId, sbyte cardOwner, int value, string eventInfo)
    {
        EventType = type;
        InstanceId = cardId;
        Owner = cardOwner;
        Data = value;
        Info = eventInfo;
    }
}