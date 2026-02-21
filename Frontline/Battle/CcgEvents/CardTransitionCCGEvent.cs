namespace Frontline.Battle.CcgEvents;

public class CardTransitionCCGEvent : CCGEventData
{
    public CCGEventType transition;

    public RegionEnum targetRegion;

    public int cardId;

    public int targetId;

    public int templateId;

    public int traitID;

    public int effectID;

    public sbyte rank;

    public sbyte playerOwner;

    public sbyte targetOwner;

    public sbyte targetSlot;

    public sbyte dir;

    public bool embark;

    public CardTransitionCCGEvent()
    {
    }

    public CardTransitionCCGEvent(CCGEventType transitionType, int deployedCardId, sbyte deployedOwner,
        int targetCardId, sbyte targetCardOwner, bool isEmbark, RegionEnum deployRegion, sbyte indexSlot, sbyte slotDir)
    {
        transition = transitionType;
        cardId = deployedCardId;
        playerOwner = deployedOwner;
        targetId = targetCardId;
        targetOwner = targetCardOwner;
        embark = isEmbark;
        targetRegion = deployRegion;
        targetSlot = indexSlot;
        dir = slotDir;
    }

    public override CCGEventType Type()
    {
        return transition;
    }
}