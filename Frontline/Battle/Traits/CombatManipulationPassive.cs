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

    public sbyte EffectType { get; set; }

    public override bool IsCombatManipulationPassive(sbyte effectID, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        if (DurationData.Charges > 0 && active.DurationData.Charges == 0)
        {
            return false;
        }

        return EffectType == effectID;
    }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            DurationData.Type == TraitDurationType.Permanent)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType, ActiveTrait active)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            ApplyStatus.IsDeterStatus(statusType) && theCard.EqualsTo(active.GetTraitSource()))
        {
            active.Deactivate(true);
        }
    }

    public override bool CanCounterAttack(CardStack target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return EffectType != 7;
    }
}