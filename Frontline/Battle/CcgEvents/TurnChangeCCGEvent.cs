namespace Frontline.Battle.CcgEvents;

public class TurnChangeCCGEvent : CCGEventData
{
    public CCGEventType ChangeType { get; set; } = CCGEventType.NewTurn;

    public sbyte PlayerIndex { get; set; }

    public TurnChangeCCGEvent()
    {
    }

    public TurnChangeCCGEvent(CCGEventType type, sbyte index)
    {
        ChangeType = type;
        PlayerIndex = index;
    }

    public override CCGEventType Type()
    {
        return ChangeType;
    }
}