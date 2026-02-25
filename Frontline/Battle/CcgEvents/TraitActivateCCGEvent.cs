using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class TraitActivateCCGEvent : CCGEventData
{
    [JsonPropertyName("traitID")]
    public int TraitId { get; set; }

    [JsonPropertyName("effectID")]
    public int EffectId { get; set; }

    [JsonPropertyName("cardID")]
    public int CardId { get; set; }

    public sbyte Owner { get; set; }

    public bool Deactivate { get; set; }

    public Region Region { get; set; } = Region.NumRegions;

    public ActiveTraitCardInfo[] Targets { get; set; }

    public TraitActivateCCGEvent()
    {
    }

    public TraitActivateCCGEvent(int baseTraitID, int traitEffectID, int sourceCardID, sbyte cardOwner,
        bool deactivateTrait)
    {
        TraitId = baseTraitID;
        EffectId = traitEffectID;
        CardId = sourceCardID;
        Owner = cardOwner;
        Deactivate = deactivateTrait;
    }

    public override CcgEventType Type()
    {
        return CcgEventType.TraitActivation;
    }
}