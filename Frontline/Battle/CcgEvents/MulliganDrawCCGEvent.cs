namespace Frontline.Battle.CcgEvents;

public class MulliganDrawCCGEvent : CCGEventData
{
    public CCGEventType drawType = CCGEventType.MulliganDraw;

    public List<MulliganDrawCCGEventCardData> CardsData = new List<MulliganDrawCCGEventCardData>();

    public sbyte owner;

    public MulliganDrawCCGEvent()
    {
    }

    public MulliganDrawCCGEvent(sbyte drawOwner)
    {
        owner = drawOwner;
    }

    public void AddDrawnCard(Card card)
    {
        if (card != null)
        {
            AddDrawnCard(card.instanceId, card.templateId, card.rank);
        }
    }

    public void AddDrawnCard(int instanceId, int templateId, sbyte rank)
    {
        CardsData.Add(new MulliganDrawCCGEventCardData(instanceId, templateId, rank));
    }

    public override CCGEventType Type()
    {
        return CCGEventType.MulliganDraw;
    }

    public override CCGEventData Sanitize(sbyte playerIndex)
    {
        if (drawType == CCGEventType.DeckDraw && owner != playerIndex)
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