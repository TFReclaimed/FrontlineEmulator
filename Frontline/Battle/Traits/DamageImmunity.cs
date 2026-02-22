namespace Frontline.Battle.Traits;

public class DamageImmunity : BaseTraitEffect
{
    public bool NormalDamage { get; set; }

    public bool BypassDamage { get; set; }

    public override bool IsDamageImmunity(bool bypass, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        return (!bypass) ? NormalDamage : BypassDamage;
    }
}