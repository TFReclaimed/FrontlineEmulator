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

    public sbyte StatusType { get; set; }

    public static bool IsDeterStatus(sbyte status)
    {
        return status == 2 || status == 1;
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (IsDeterStatus(StatusType))
        {
            ActiveTrait activeTrait = null;
            for (int i = 0; i < card.ActiveData.ActiveTraits.Count; i++)
            {
                activeTrait = card.ActiveData.ActiveTraits[i];
                if (activeTrait.GetTraitInfo().Deterable)
                {
                    activeTrait.Detered = true;
                }
            }
        }

        base.Apply(card, source, active);
        GameState.CardGainedStatus(card, source, StatusType);
    }

    public override void Deactivate(ActiveTrait active)
    {
        if (IsDeterStatus(StatusType))
        {
            ActiveTrait activeTrait = null;
            ActiveCardData activeData = active.GetTraitTarget().ActiveData;
            bool flag = false;
            for (int i = 0; i < activeData.ActiveTraits.Count; i++)
            {
                activeTrait = activeData.ActiveTraits[i];
                if (activeTrait != active && (activeTrait.GetTraitInfo().IsStatusEffect(2, activeTrait) ||
                                              activeTrait.GetTraitInfo().IsStatusEffect(1, activeTrait)))
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                for (int j = 0; j < active.GetTraitTarget().ActiveData.ActiveTraits.Count; j++)
                {
                    activeTrait = active.GetTraitTarget().ActiveData.ActiveTraits[j];
                    activeTrait.Detered = !activeTrait.EmbarkedCheck();
                }

                activeTrait.GetTraitTarget().OnRemovedDeter();
            }
        }

        base.Deactivate(active);
    }

    public override bool CanDeploy(CardStack target, Region region)
    {
        if (StatusType == 3 && region == Region.Control)
        {
            return false;
        }

        return true;
    }

    public override bool CanMove(Region target, sbyte cardOwner, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return (StatusType != 3 || target != Region.Control) && StatusType != 1 && StatusType != 4;
    }

    public override bool CanAttack(CardStack target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return StatusType != 1;
    }

    public override bool CanCounterAttack(CardStack target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return StatusType != 1;
    }

    public override bool IsStatusEffect(sbyte effectID, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        return effectID == StatusType;
    }
}