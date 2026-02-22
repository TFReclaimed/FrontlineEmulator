namespace Frontline.Battle.Traits;

public class ReactiveDamage : BaseTraitEffect
{
    public sbyte Damage { get; set; }

    public sbyte Bypass { get; set; }

    public TraitTargetType AttackerType { get; set; } = TraitTargetType.AnyType;

    public override void CardAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (target.EqualsTo(active.GetTraitTarget()) && (!Deterable || !active.Detered) &&
            (DurationData.Charges <= 0 || active.DurationData.Charges != 0) &&
            TraitTargeting.DoesMatchType(AttackerType, TargetTypeMod.NumMods, 0, attacker))
        {
            sbyte attack = Damage;
            sbyte b = Bypass;
            if (Damage == -1)
            {
                attack = attacker.GetCurrentHealth(false);
            }
            else if (Damage > 0 && active.DataValue > 0)
            {
                attack = (sbyte) active.DataValue;
            }

            if (Bypass == -1)
            {
                b = attacker.GetCurrentHealth(false);
            }
            else if (Bypass > 0 && active.DataValue > 0)
            {
                b = (sbyte) active.DataValue;
            }

            attacker.TakeDamage(attack, b, target, true);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }
}