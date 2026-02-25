using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class TraitInfoCCGEvent : CCGEventData
{
    public CcgEventType InfoType { get; set; }

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

    public Region Region { get; set; } = Region.NumRegions;

    public ActiveTraitCardInfo[] Targets { get; set; }

    public TraitInfoCCGEvent()
    {
    }

    public TraitInfoCCGEvent(CcgEventType type, int baseTraitID, int traitEffectID, int targetInstanceID,
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

    public override CcgEventType Type()
    {
        return InfoType;
    }
}