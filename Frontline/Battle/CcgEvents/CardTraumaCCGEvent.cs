using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CardTraumaCCGEvent : CCGEventData
{
    public CCGEventType TraumaType { get; set; }

    public int Health { get; set; }

    [JsonPropertyName("sourceCardID")]
    public int SourceCardId { get; set; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; set; }

    public sbyte SourceOwner { get; set; }

    public sbyte TargetOwner { get; set; }

    public CardTraumaCCGEvent()
    {
    }

    public CardTraumaCCGEvent(CCGEventType type, int healthDelta, int sourceId, sbyte sourceCardOwner, int targetId,
        sbyte targetCardOwner)
    {
        TraumaType = type;
        Health = healthDelta;
        SourceCardId = sourceId;
        TargetCardId = targetId;
        SourceOwner = sourceCardOwner;
        TargetOwner = targetCardOwner;
    }

    public override CCGEventType Type()
    {
        return TraumaType;
    }
}