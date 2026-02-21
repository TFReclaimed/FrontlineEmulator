namespace Frontline.Battle.CcgEvents;

public class CardDrawCCGEvent : CCGEventData
{
    public CCGEventType DrawType { get; }

    public int CardId { get; }

    public int TemplateId { get; }

    public sbyte Rank { get; }

    public sbyte Owner { get; }

    public CardDrawCCGEvent()
    {
    }

    public CardDrawCCGEvent(CCGEventType type, int drawnId, sbyte drawOwner, int template, sbyte cardRank)
    {
        DrawType = type;
        CardId = drawnId;
        Owner = drawOwner;
        TemplateId = template;
        Rank = cardRank;
    }

    public override CCGEventType Type()
    {
        return DrawType;
    }

    public override CCGEventData Sanitize(sbyte playerIndex)
    {
        if (DrawType == CCGEventType.DeckDraw && Owner != playerIndex)
        {
            return new CardDrawCCGEvent(DrawType, CardId, Owner, 0, 0);
        }

        return base.Sanitize(playerIndex);
    }
}