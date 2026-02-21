namespace Frontline.Battle.CcgEvents;

public class DiscardEffectCCGEvent : CCGEventData
{
    public sbyte PlayerIndex { get; }

    public MulliganDrawCCGEventCardData[] CardsInfo { get; }

    public int EffectId { get; set; }

    public int TraitId { get; set; }

    public DiscardEffectCCGEvent()
    {
    }

    public DiscardEffectCCGEvent(sbyte owner, MulliganDrawCCGEventCardData[] data)
    {
        PlayerIndex = owner;
        CardsInfo = data;
    }

    public override CCGEventType Type()
    {
        return CCGEventType.CardDiscard;
    }
}