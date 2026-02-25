using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class TraitInfoCcgEvent : CcgEventData
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

    public TraitInfoCcgEvent(CcgEventType type, int baseTraitId, int traitEffectId, int targetInstanceId,
        sbyte targetPlayerIdx, int sourceInstanceId, sbyte sourcePlayerIdx, sbyte info)
    {
        InfoType = type;
        TraitId = baseTraitId;
        EffectId = traitEffectId;
        TargetCardId = targetInstanceId;
        TargetOwner = targetPlayerIdx;
        SourceCardId = sourceInstanceId;
        SourceOwner = sourcePlayerIdx;
        Data = info;
    }
}