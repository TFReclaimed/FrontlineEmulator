namespace Frontline.Battle.Traits;

public class ApplyStatus : BaseTraitEffect
{
    public const sbyte stun = 1;

    public const sbyte deter = 2;

    public const sbyte defender = 3;

    public const sbyte immobilize = 4;

    public const sbyte cannotTargetBurn = 5;

    public const sbyte cannotTargetTrait = 6;

    public const sbyte cannotTargetSecret = 7;

    public const sbyte operative = 8;

    public sbyte statusType;

    public static bool IsDeterStatus(sbyte status)
    {
        return status == 2 || status == 1;
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (IsDeterStatus(statusType))
        {
            ActiveTrait activeTrait = null;
            for (int i = 0; i < card.activeData.activeTraits.Count; i++)
            {
                activeTrait = card.activeData.activeTraits[i];
                if (activeTrait.GetTraitInfo().deterable)
                {
                    activeTrait.detered = true;
                }
            }
        }

        base.Apply(card, source, active);
        GameState.CardGainedStatus(card, source, statusType);
    }

    public override void Deactivate(ActiveTrait active)
    {
        if (IsDeterStatus(statusType))
        {
            ActiveTrait activeTrait = null;
            ActiveCardData activeData = active.GetTraitTarget().activeData;
            bool flag = false;
            for (int i = 0; i < activeData.activeTraits.Count; i++)
            {
                activeTrait = activeData.activeTraits[i];
                if (activeTrait != active && (activeTrait.GetTraitInfo().IsStatusEffect(2, activeTrait) ||
                                              activeTrait.GetTraitInfo().IsStatusEffect(1, activeTrait)))
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                for (int j = 0; j < active.GetTraitTarget().activeData.activeTraits.Count; j++)
                {
                    activeTrait = active.GetTraitTarget().activeData.activeTraits[j];
                    activeTrait.detered = !activeTrait.EmbarkedCheck();
                }

                activeTrait.GetTraitTarget().OnRemovedDeter();
            }
        }

        base.Deactivate(active);
    }

    public override bool CanDeploy(CardStack target, RegionEnum region)
    {
        if (statusType == 3 && region == RegionEnum.Control)
        {
            return false;
        }

        return true;
    }

    public override bool CanMove(RegionEnum target, sbyte cardOwner, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return true;
        }

        return (statusType != 3 || target != RegionEnum.Control) && statusType != 1 && statusType != 4;
    }

    public override bool CanAttack(CardStack target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return true;
        }

        return statusType != 1;
    }

    public override bool CanCounterAttack(CardStack target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return true;
        }

        return statusType != 1;
    }

    public override bool IsStatusEffect(sbyte effectID, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return false;
        }

        return effectID == statusType;
    }
}