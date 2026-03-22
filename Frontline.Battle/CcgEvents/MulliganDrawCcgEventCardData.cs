namespace Frontline.Battle.CcgEvents;

public class MulliganDrawCcgEventCardData : CcgEventData
{
    public int InstanceId { get; set; }

    public int TemplateId { get; set; }

    public sbyte CardRank { get; set; }

    public MulliganDrawCcgEventCardData(int instanceId, int templateId, sbyte cardRank)
    {
        InstanceId = instanceId;
        TemplateId = templateId;
        CardRank = cardRank;
    }
}