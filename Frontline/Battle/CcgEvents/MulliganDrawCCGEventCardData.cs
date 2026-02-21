namespace Frontline.Battle.CcgEvents;

public class MulliganDrawCCGEventCardData : CCGEventData
{
    public int InstanceId;

    public int TemplateId;

    public sbyte CardRank;

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