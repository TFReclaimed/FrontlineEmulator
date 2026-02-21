namespace Frontline.Battle.CcgEvents;

public class TurnChangeCCGEvent : CCGEventData
{
    public CCGEventType changeType = CCGEventType.NewTurn;

    public sbyte playerIndex;

    public TurnChangeCCGEvent()
    {
    }

    public TurnChangeCCGEvent(CCGEventType type, sbyte index)
    {
        changeType = type;
        playerIndex = index;
    }

    public override CCGEventType Type()
    {
        return changeType;
    }
}