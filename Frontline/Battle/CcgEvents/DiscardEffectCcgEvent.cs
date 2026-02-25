namespace Frontline.Battle.CcgEvents;

public class DiscardEffectCcgEvent : CcgEventData
{
    public sbyte PlayerIndex { get; set; }

    public MulliganDrawCcgEventCardData[] CardsInfo { get; set; }

    public int EffectId { get; set; }

    public int TraitId { get; set; }

    public DiscardEffectCcgEvent(sbyte owner, MulliganDrawCcgEventCardData[] data)
    {
        PlayerIndex = owner;
        CardsInfo = data;
    }
}