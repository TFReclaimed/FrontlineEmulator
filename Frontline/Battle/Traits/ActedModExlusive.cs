namespace Frontline.Battle.Traits;

public class ActedModExlusive : BaseTraitEffect
{
    public bool deploy;

    public bool attack;

    public bool move;

    public bool activate;

    public override void Move(CardStack location, RegionEnum region, bool embark, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) && move)
        {
            EntityCard entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(10);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }

    public override void Attack(Card target, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) &&
            attack)
        {
            EntityCard entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(12);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }

    public override void ActivateAction(CardStack location, RegionEnum region, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) &&
            activate)
        {
            EntityCard entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(6);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }
}