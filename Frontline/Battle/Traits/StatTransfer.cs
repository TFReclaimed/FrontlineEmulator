namespace Frontline.Battle.Traits;

public class StatTransfer : BaseTraitEffect
{
    public bool attack;

    public bool bypassDefense;

    public bool defense;

    public bool health;

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (targets.scope != 0 && targets.scope != TraitTargetScope.UnitStack)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }

    public override sbyte GetAttackBonus(Card target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (attack)
        {
            return active.GetTraitSource().GetCurrentAttack(target, false);
        }

        return 0;
    }

    public override sbyte GetBypassDefenseBonus(Card target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (bypassDefense)
        {
            return active.GetTraitSource().GetCurrentBypassDefense(target, false);
        }

        return 0;
    }

    public override sbyte GetDefenseBonus(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (defense)
        {
            return active.GetTraitSource().GetCurrentDefense(false);
        }

        return 0;
    }

    public override sbyte GetHealthBonus(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (health)
        {
            return active.GetTraitSource().GetCurrentHealth(false);
        }

        return 0;
    }
}