namespace Frontline.Battle.CcgEvents;

public class TurnChangeCCGEvent : CCGEventData
{
    public CcgEventType ChangeType { get; set; } = CcgEventType.NewTurn;

    public sbyte PlayerIndex { get; set; }

    public TurnChangeCCGEvent()
    {
    }

    public TurnChangeCCGEvent(CcgEventType type, sbyte index)
    {
        ChangeType = type;
        PlayerIndex = index;
    }

    public override CcgEventType Type()
    {
        return ChangeType;
    }
}