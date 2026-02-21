namespace Frontline.Battle.Traits;

public class CombatManipulationPassive : BaseTraitEffect
{
    public const sbyte Stealth = 1;

    public const sbyte IgnoreStealth = 2;

    public const sbyte Dodge = 3;

    public const sbyte Sniper = 4;

    public const sbyte IgnoreSniper = 5;

    public const sbyte Block = 6;

    public const sbyte DisableCounter = 7;

    public const sbyte DamageConvertAP = 8;

    public const sbyte DamageConvertNormal = 9;

    public sbyte effectType;

    public override bool IsCombatManipulationPassive(sbyte effectID, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return false;
        }

        if (durationData.charges > 0 && active.durationData.charges == 0)
        {
            return false;
        }

        return effectType == effectID;
    }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack &&
            durationData.type == TraitDurationType.Permanent)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType, ActiveTrait active)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack &&
            ApplyStatus.IsDeterStatus(statusType) && theCard.EqualsTo(active.GetTraitSource()))
        {
            active.Deactivate(true);
        }
    }

    public override bool CanCounterAttack(CardStack target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return true;
        }

        return effectType != 7;
    }
}