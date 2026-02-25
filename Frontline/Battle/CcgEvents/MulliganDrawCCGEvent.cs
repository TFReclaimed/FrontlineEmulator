namespace Frontline.Battle.CcgEvents;

public class MulliganDrawCCGEvent : CCGEventData
{
    public CcgEventType DrawType { get; set; } = CcgEventType.MulliganDraw;

    public List<MulliganDrawCCGEventCardData> CardsData { get; set; } = new List<MulliganDrawCCGEventCardData>();

    public sbyte Owner { get; set; }

    public MulliganDrawCCGEvent()
    {
    }

    public MulliganDrawCCGEvent(sbyte drawOwner)
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
        CardsData.Add(new MulliganDrawCCGEventCardData(instanceId, templateId, rank));
    }

    public override CcgEventType Type()
    {
        return CcgEventType.MulliganDraw;
    }

    public override CCGEventData Sanitize(sbyte playerIndex)
    {
        if (DrawType == CcgEventType.DeckDraw && Owner != playerIndex)
        {
            MulliganDrawCCGEvent mulliganDrawCCGEvent = new MulliganDrawCCGEvent();
            for (int i = 0; i < CardsData.Count; i++)
            {
                mulliganDrawCCGEvent.AddDrawnCard(CardsData[i].InstanceId, 0, 0);
            }

            return mulliganDrawCCGEvent;
        }

        return base.Sanitize(playerIndex);
    }
}