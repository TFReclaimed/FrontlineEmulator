namespace Frontline.Battle.CcgEvents;

public class CardDrawCCGEvent : CCGEventData
{
    public CCGEventType DrawType { get; set; }

    public int CardId { get; set; }

    public int TemplateId { get; set; }

    public sbyte Rank { get; set; }

    public sbyte Owner { get; set; }

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