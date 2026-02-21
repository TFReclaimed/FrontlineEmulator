using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class TraitInfoCCGEvent : CCGEventData
{
    public CCGEventType InfoType { get; set; }

    [JsonPropertyName("traitID")]
    public int TraitId { get; set; }

    [JsonPropertyName("effectID")]
    public int EffectId { get; set; }

    [JsonPropertyName("targetCardID")]
    public int TargetCardId { get; set; }

    public sbyte TargetOwner { get; set; }

    [JsonPropertyName("sourceCardID")]
    public int SourceCardId { get; set; }

    public sbyte SourceOwner { get; set; }

    public sbyte Data { get; set; }

    public RegionEnum Region { get; set; } = RegionEnum.NumRegions;

    public ActiveTraitCardInfo[] Targets { get; set; }

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