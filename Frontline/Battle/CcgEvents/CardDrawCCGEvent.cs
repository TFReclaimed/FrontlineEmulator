namespace Frontline.Battle.CcgEvents;

public class CardDrawCCGEvent : CCGEventData
{
    public CCGEventType drawType;

    public int cardId;

    public int templateId;

    public sbyte rank;

    public sbyte owner;

    public CardDrawCCGEvent()
    {
    }

    public CardDrawCCGEvent(CCGEventType type, int drawnId, sbyte drawOwner, int template, sbyte cardRank)
    {
        drawType = type;
        cardId = drawnId;
        owner = drawOwner;
        templateId = template;
        rank = cardRank;
    }

    public override CCGEventType Type()
    {
        return drawType;
    }

    public override CCGEventData Sanitize(sbyte playerIndex)
    {
        if (drawType == CCGEventType.DeckDraw && owner != playerIndex)
        {
            return new CardDrawCCGEvent(drawType, cardId, owner, 0, 0);
        }

        return base.Sanitize(playerIndex);
    }
}