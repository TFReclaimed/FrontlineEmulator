namespace Frontline.Battle.Traits;

public class CombatManipulationPassive : BaseTraitEffect
{
    public CombatManipulationPassiveType EffectType { get; set; }

    public override bool IsCombatManipulationPassive(CombatManipulationPassiveType effectId, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        if (DurationData.Charges > 0 && active.DurationData.Charges == 0)
        {
            return false;
        }

        return EffectType == effectId;
    }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            DurationData.Type == TraitDurationType.Permanent)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType, ActiveTrait active)
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

        return EffectType != CombatManipulationPassiveType.DisableCounter;
    }
}

public enum CombatManipulationPassiveType
{
    None = 0,
    Stealth = 1,
    IgnoreStealth = 2,
    Dodge = 3,
    Sniper = 4,
    IgnoreSniper = 5,
    Block = 6,
    DisableCounter = 7,
    DamageConvertAp = 8,
    DamageConvertNormal = 9
}