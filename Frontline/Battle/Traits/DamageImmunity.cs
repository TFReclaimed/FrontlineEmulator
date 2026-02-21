namespace Frontline.Battle.Traits;

public class DamageImmunity : BaseTraitEffect
{
    public bool normalDamage;

    public bool bypassDamage;

    public override bool IsDamageImmunity(bool bypass, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        return (!bypass) ? normalDamage : bypassDamage;
    }
}