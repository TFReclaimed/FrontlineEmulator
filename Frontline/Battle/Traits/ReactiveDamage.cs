namespace Frontline.Battle.Traits;

public class ReactiveDamage : BaseTraitEffect
{
    public sbyte damage;

    public sbyte bypass;

    public TraitTargetType attackerType = TraitTargetType.AnyType;

    public override void CardAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (target.EqualsTo(active.GetTraitTarget()) && (!deterable || !active.detered) &&
            (durationData.charges <= 0 || active.durationData.charges != 0) &&
            TraitTargeting.DoesMatchType(attackerType, TargetTypeMod.NumMods, 0, attacker))
        {
            sbyte attack = damage;
            sbyte b = bypass;
            if (damage == -1)
            {
                attack = attacker.GetCurrentHealth(false);
            }
            else if (damage > 0 && active.dataValue > 0)
            {
                attack = (sbyte) active.dataValue;
            }

            if (bypass == -1)
            {
                b = attacker.GetCurrentHealth(false);
            }
            else if (bypass > 0 && active.dataValue > 0)
            {
                b = (sbyte) active.dataValue;
            }

            attacker.TakeDamage(attack, b, target, true);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }
}