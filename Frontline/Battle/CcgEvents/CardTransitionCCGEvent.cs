using System.Text.Json.Serialization;

namespace Frontline.Battle.CcgEvents;

public class CardTransitionCCGEvent : CCGEventData
{
    public CCGEventType Transition { get; set; }

    public RegionEnum TargetRegion { get; }

    public int CardId { get; }

    public int TargetId { get; set; }

    public int TemplateId { get; set; }

    [JsonPropertyName("traitID")]
    public int TraitId { get; set; }

    [JsonPropertyName("effectID")]
    public int EffectId { get; set; }

    public sbyte Rank { get; set; }

    public sbyte PlayerOwner { get; }

    public sbyte TargetOwner { get; set; }

    public sbyte TargetSlot { get; }

    public sbyte Dir { get; }

    public bool Embark { get; set; }

    public CardTransitionCCGEvent()
    {
    }

    public CardTransitionCCGEvent(CCGEventType transitionType, int deployedCardId, sbyte deployedOwner,
        int targetCardId, sbyte targetCardOwner, bool isEmbark, RegionEnum deployRegion, sbyte indexSlot, sbyte slotDir)
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

    public override CCGEventType Type()
    {
        return Transition;
    }
}