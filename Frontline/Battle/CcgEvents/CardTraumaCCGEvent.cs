using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CardTraumaCCGEvent : CCGEventData
{
    public CCGEventType TraumaType { get; }

    public int Health { get; }

    [JsonPropertyName("sourceCardID")]
    public int SourceCardId { get; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; }

    public sbyte SourceOwner { get; }

    public sbyte TargetOwner { get; }

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