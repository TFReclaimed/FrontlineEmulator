namespace Frontline.Battle.Traits;

public class ActedModPassive : BaseTraitEffect
{
    public bool deploy;

    public bool attack;

    public bool move;

    public bool activate;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte b = 0;
        EntityCard entityCard = (EntityCard) card;
        bool flag = entityCard.IsCardTraitsDetered();
        if (!active.GetTraitInfo().Deterable)
        {
            flag = false;
        }

        if (active.HasCharges() || active.HasDuration())
        {
            base.Apply(card, source, active);
            if (deploy && !flag)
            {
                if (deploy)
                {
                    b = (sbyte) ((byte) b | 1);
                }

                if (attack)
                {
                    b = (sbyte) ((byte) b | 2);
                }

                if (move)
                {
                    b = (sbyte) ((byte) b | 4);
                }

                if (activate)
                {
                    b = (sbyte) ((byte) b | 8);
                }

                entityCard.ClearActed(b);
            }
        }
        else if (!flag)
        {
            if (deploy)
            {
                b = (sbyte) ((byte) b | 1);
            }

            if (attack)
            {
                b = (sbyte) ((byte) b | 2);
            }

            if (move)
            {
                b = (sbyte) ((byte) b | 4);
            }

            if (activate)
            {
                b = (sbyte) ((byte) b | 8);
            }

            entityCard.ClearActed(b);
        }
    }

    public override void Move(CardStack location, RegionEnum region, bool embark, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) && move)
        {
            EntityCard entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(14);
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
            entityCard.ClearActed(14);
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
            entityCard.ClearActed(14);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            DurationData.Type == TraitDurationType.Permanent)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }
}