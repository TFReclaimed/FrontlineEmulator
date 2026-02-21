namespace Frontline.Battle.Traits;

public class StatModifierPassive : BaseTraitEffect
{
    public TraitTargetType targetType = TraitTargetType.AnyType;

    public sbyte attack;

    public sbyte bypassDefense;

    public sbyte defense;

    public sbyte health;

    public sbyte command;

    public override sbyte GetAttackBonus(Card target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (TraitTargeting.DoesMatchType(targetType, TargetTypeMod.NumMods, 0, target))
        {
            sbyte result = attack;
            if (attack != 0 && active.dataValue != 0)
            {
                result = (sbyte) active.dataValue;
            }

            return result;
        }

        return 0;
    }

    public override sbyte GetBypassDefenseBonus(Card target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (TraitTargeting.DoesMatchType(targetType, TargetTypeMod.NumMods, 0, target))
        {
            sbyte result = bypassDefense;
            if (bypassDefense != 0 && active.dataValue != 0)
            {
                result = (sbyte) active.dataValue;
            }

            return result;
        }

        return 0;
    }

    public override sbyte GetDefenseBonus(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        sbyte result = defense;
        if (defense != 0 && active.dataValue != 0)
        {
            result = (sbyte) active.dataValue;
        }

        return result;
    }

    public override sbyte GetHealthBonus(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        sbyte result = health;
        if (health != 0 && active.dataValue != 0)
        {
            result = (sbyte) active.dataValue;
        }

        return result;
    }

    public override sbyte GetCommandMod(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        sbyte result = command;
        if (command != 0 && active.dataValue != 0)
        {
            result = (sbyte) active.dataValue;
        }

        return result;
    }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack &&
            durationData.type == TraitDurationType.Permanent)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }

    public override void OnCardMovedEvent(Card parent, Card movedCard, CardStack location, RegionEnum region,
        RegionEnum origin)
    {
        if (targets.scope == TraitTargetScope.Self || targets.scope == TraitTargetScope.UnitStack ||
            durationData.type != TraitDurationType.Permanent || targets.area != TargetableArea.CurrentRegion ||
            region == origin)
        {
            return;
        }

        if (parent.EqualsTo(movedCard))
        {
            CheckGlobalApply(movedCard, region, true);
            return;
        }

        RegionEnum traitActorRegion = GameState.GetTraitActorRegion(parent.activeData.owner, parent.instanceId);
        if (traitActorRegion == region)
        {
            CheckAndApplyTrait(movedCard, parent, false, false);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType, ActiveTrait active)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack &&
            ApplyStatus.IsDeterStatus(statusType) && active.GetTraitSource() != null &&
            theCard.EqualsTo(active.GetTraitSource()))
        {
            active.Deactivate(true);
        }
    }

    public override void Move(CardStack location, RegionEnum region, bool embark, ActiveTrait active)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack &&
            durationData.type == TraitDurationType.Permanent && targets.area == TargetableArea.CurrentRegion)
        {
            active.Deactivate(true);
        }
    }

    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack &&
            durationData.type == TraitDurationType.Permanent && active.GetTraitSource() != null &&
            deadCard.EqualsTo(active.GetTraitSource()))
        {
            active.Deactivate(true);
        }
    }

    public override void CardMoved(Card theCard, CardStack target, RegionEnum destination, RegionEnum origin,
        ActiveTrait active)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack &&
            targets.area == TargetableArea.CurrentRegion && durationData.type == TraitDurationType.Permanent &&
            destination != origin && active.GetTraitSource() != null && theCard.EqualsTo(active.GetTraitSource()) &&
            !theCard.EqualsTo(active.GetTraitTarget()))
        {
            active.Deactivate(true);
        }
    }
}