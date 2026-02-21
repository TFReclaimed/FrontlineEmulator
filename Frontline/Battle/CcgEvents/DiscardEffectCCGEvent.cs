namespace Frontline.Battle.CcgEvents;

public class DiscardEffectCCGEvent : CCGEventData
{
    public sbyte playerIndex;

    public MulliganDrawCCGEventCardData[] cardsInfo;

    public int effectId;

    public int traitId;

    public DiscardEffectCCGEvent()
    {
    }

    public DiscardEffectCCGEvent(sbyte owner, MulliganDrawCCGEventCardData[] data)
    {
        playerIndex = owner;
        cardsInfo = data;
    }

    public override CCGEventType Type()
    {
        return CCGEventType.CardDiscard;
    }
}