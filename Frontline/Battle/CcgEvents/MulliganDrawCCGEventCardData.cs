namespace Frontline.Battle.CcgEvents;

public class MulliganDrawCCGEventCardData : CCGEventData
{
    public int InstanceId { get; set; }

    public int TemplateId { get; set; }

    public sbyte CardRank { get; set; }

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