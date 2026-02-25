namespace Frontline.Battle.CcgEvents;

public class CardDrawCcgEvent : CcgEventData
{
    public CcgEventType DrawType { get; set; }

    public int CardId { get; set; }

    public int TemplateId { get; set; }

    public sbyte Rank { get; set; }

    public sbyte Owner { get; set; }

    public CardDrawCcgEvent(CcgEventType type, int drawnId, sbyte drawOwner, int template, sbyte cardRank)
    {
        DrawType = type;
        CardId = drawnId;
        Owner = drawOwner;
        TemplateId = template;
        Rank = cardRank;
    }
}