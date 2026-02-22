using System.Text.Json.Serialization;

namespace Frontline.Battle.Traits;

public class StatTransfer : BaseTraitEffect
{
    [JsonPropertyName("attack")]
    public bool IsAttack { get; set; }

    public bool BypassDefense { get; set; }

    public bool Defense { get; set; }

    public bool Health { get; set; }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }

    public override sbyte GetAttackBonus(Card target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (IsAttack)
        {
            return active.GetTraitSource().GetCurrentAttack(target, false);
        }

        return 0;
    }

    public override sbyte GetBypassDefenseBonus(Card target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (BypassDefense)
        {
            return active.GetTraitSource().GetCurrentBypassDefense(target, false);
        }

        return 0;
    }

    public override sbyte GetDefenseBonus(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (Defense)
        {
            return active.GetTraitSource().GetCurrentDefense(false);
        }

        return 0;
    }

    public override sbyte GetHealthBonus(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (Health)
        {
            return active.GetTraitSource().GetCurrentHealth(false);
        }

        return 0;
    }
}