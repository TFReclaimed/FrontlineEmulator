namespace Frontline.Battle.CcgEvents;

public class TurnChangeCcgEvent : CcgEventData
{
    public CcgEventType ChangeType { get; set; } = CcgEventType.NewTurn;

    public sbyte PlayerIndex { get; set; }

    public TurnChangeCcgEvent(CcgEventType type, sbyte index)
    {
        ChangeType = type;
        PlayerIndex = index;
    }
}