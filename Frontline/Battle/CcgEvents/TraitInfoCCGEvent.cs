using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class TraitInfoCCGEvent : CCGEventData
{
    public CCGEventType InfoType { get; }

    [JsonPropertyName("traitID")]
    public int TraitId { get; }

    [JsonPropertyName("effectID")]
    public int EffectId { get; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; }

    public sbyte TargetOwner { get; }

    [JsonPropertyName("sourceCardID")]
    public int SourceCardId { get; }

    public sbyte SourceOwner { get; }

    public sbyte Data { get; }

    public RegionEnum Region { get; } = RegionEnum.NumRegions;

    public ActiveTraitCardInfo[] Targets { get; }

    public TraitInfoCCGEvent()
    {
    }

    public TraitInfoCCGEvent(CCGEventType type, int baseTraitID, int traitEffectID, int targetInstanceID,
        sbyte targetPlayerIdx, int sourceInstanceID, sbyte sourcePlayerIdx, sbyte info)
    {
        InfoType = type;
        TraitId = baseTraitID;
        EffectId = traitEffectID;
        TargetCardId = targetInstanceID;
        TargetOwner = targetPlayerIdx;
        SourceCardId = sourceInstanceID;
        SourceOwner = sourcePlayerIdx;
        Data = info;
    }

    public override CCGEventType Type()
    {
        return InfoType;
    }
}