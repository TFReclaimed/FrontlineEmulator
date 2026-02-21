namespace Frontline.Battle.CcgEvents;

public class MulliganDrawCCGEventCardData : CCGEventData
{
    public int InstanceId { get; }

    public int TemplateId { get; }

    public sbyte CardRank { get; }

    public MulliganDrawCCGEventCardData()
    {
    }

    public MulliganDrawCCGEventCardData(int instanceId, int templateId, sbyte cardRank)
    {
        InstanceId = instanceId;
        TemplateId = templateId;
        CardRank = cardRank;
    }
}