using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CardTransitionCcgEvent : CcgEventData
{
    public CcgEventType Transition { get; set; }

    public Region TargetRegion { get; set; }

    public int CardId { get; set; }

    public int TargetId { get; set; }

    public int TemplateId { get; set; }

    [JsonPropertyName("traitID")]
    public int TraitId { get; set; }

    [JsonPropertyName("effectID")]
    public int EffectId { get; set; }

    public sbyte Rank { get; set; }

    public sbyte PlayerOwner { get; set; }

    public sbyte TargetOwner { get; set; }

    public sbyte TargetSlot { get; set; }

    public sbyte Dir { get; set; }

    public bool Embark { get; set; }

    public CardTransitionCcgEvent(CcgEventType transitionType, int deployedCardId, sbyte deployedOwner,
        int targetCardId, sbyte targetCardOwner, bool isEmbark, Region deployRegion, sbyte indexSlot, sbyte slotDir)
    {
        Transition = transitionType;
        CardId = deployedCardId;
        PlayerOwner = deployedOwner;
        TargetId = targetCardId;
        TargetOwner = targetCardOwner;
        Embark = isEmbark;
        TargetRegion = deployRegion;
        TargetSlot = indexSlot;
        Dir = slotDir;
    }
}