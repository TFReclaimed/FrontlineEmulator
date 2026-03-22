using System.Text.Json.Serialization;

namespace Frontline.Battle.Traits;

public class StatModifierPassive : BaseTraitEffect
{
    public TraitTargetType TargetType { get; set; } = TraitTargetType.AnyType;

    [JsonPropertyName("attack")]
    public sbyte IsAttack { get; set; }

    public sbyte BypassDefense { get; set; }

    public sbyte Defense { get; set; }

    public sbyte Health { get; set; }

    public sbyte Command { get; set; }

    public override sbyte GetAttackBonus(Card? target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (!TraitTargeting.DoesMatchType(TargetType, TargetTypeMod.NumMods, 0, target))
        {
            return 0;
        }

        var result = IsAttack;
        if (IsAttack != 0 && active.DataValue != 0)
        {
            result = (sbyte) active.DataValue;
        }

        return result;
    }

    public override sbyte GetBypassDefenseBonus(Card? target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (!TraitTargeting.DoesMatchType(TargetType, TargetTypeMod.NumMods, 0, target))
        {
            return 0;
        }

        var result = BypassDefense;
        if (BypassDefense != 0 && active.DataValue != 0)
        {
            result = (sbyte) active.DataValue;
        }

        return result;
    }

    public override sbyte GetDefenseBonus(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        var result = Defense;
        if (Defense != 0 && active.DataValue != 0)
        {
            result = (sbyte) active.DataValue;
        }

        return result;
    }

    public override sbyte GetHealthBonus(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        var result = Health;
        if (Health != 0 && active.DataValue != 0)
        {
            result = (sbyte) active.DataValue;
        }

        return result;
    }

    public override sbyte GetCommandMod(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        var result = Command;
        if (Command != 0 && active.DataValue != 0)
        {
            result = (sbyte) active.DataValue;
        }

        return result;
    }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            DurationData.Type == TraitDurationType.Permanent)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }

    public override void OnCardMovedEvent(Card parent, Card movedCard, CardStack location, Region region,
        Region origin)
    {
        if (Targets.Scope == TraitTargetScope.Self || Targets.Scope == TraitTargetScope.UnitStack ||
            DurationData.Type != TraitDurationType.Permanent || Targets.Area != TargetableArea.CurrentRegion ||
            region == origin)
        {
            return;
        }

        if (parent.EqualsTo(movedCard))
        {
            CheckGlobalApply(movedCard, region, true);
            return;
        }

        var traitActorRegion = GameState.GetTraitActorRegion(parent.ActiveData.Owner, parent.InstanceId);
        if (traitActorRegion == region)
        {
            CheckAndApplyTrait(movedCard, parent, false, false);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType, ActiveTrait active)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            ApplyStatus.IsDeterStatus(statusType) && active.GetTraitSource() != null &&
            theCard.EqualsTo(active.GetTraitSource()))
        {
            active.Deactivate(true);
        }
    }

    public override void Move(CardStack location, Region region, bool embark, ActiveTrait active)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            DurationData.Type == TraitDurationType.Permanent && Targets.Area == TargetableArea.CurrentRegion)
        {
            active.Deactivate(true);
        }
    }

    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            DurationData.Type == TraitDurationType.Permanent && active.GetTraitSource() != null &&
            deadCard.EqualsTo(active.GetTraitSource()))
        {
            active.Deactivate(true);
        }
    }

    public override void CardMoved(Card theCard, CardStack target, Region destination, Region origin,
        ActiveTrait active)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            Targets.Area == TargetableArea.CurrentRegion && DurationData.Type == TraitDurationType.Permanent &&
            destination != origin && active.GetTraitSource() != null && theCard.EqualsTo(active.GetTraitSource()) &&
            !theCard.EqualsTo(active.GetTraitTarget()))
        {
            active.Deactivate(true);
        }
    }
}