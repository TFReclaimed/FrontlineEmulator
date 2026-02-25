namespace Frontline.Battle.CcgEvents;

public class DiscardEffectCCGEvent : CCGEventData
{
    public sbyte PlayerIndex { get; set; }

    public MulliganDrawCCGEventCardData[] CardsInfo { get; set; }

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

    public override CcgEventType Type()
    {
        return CcgEventType.CardDiscard;
    }
}