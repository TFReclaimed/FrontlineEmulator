namespace Frontline.Battle.CcgEvents;

public class MulliganDrawCcgEvent : CcgEventData
{
    public CcgEventType DrawType { get; set; } = CcgEventType.MulliganDraw;

    public List<MulliganDrawCcgEventCardData> CardsData { get; set; } = [];

    public sbyte Owner { get; set; }

    public MulliganDrawCcgEvent(sbyte drawOwner)
    {
        Owner = drawOwner;
    }

    public void AddDrawnCard(Card card)
    {
        if (card != null)
        {
            AddDrawnCard(card.InstanceId, card.TemplateId, card.Rank);
        }
    }

    public void AddDrawnCard(int instanceId, int templateId, sbyte rank)
    {
        CardsData.Add(new MulliganDrawCcgEventCardData(instanceId, templateId, rank));
    }
}