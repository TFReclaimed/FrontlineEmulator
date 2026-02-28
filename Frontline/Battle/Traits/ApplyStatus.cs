namespace Frontline.Battle.Traits;

public class ApplyStatus : BaseTraitEffect
{
    public ApplyStatusTraitStatusType StatusType { get; set; }

    public static bool IsDeterStatus(ApplyStatusTraitStatusType status)
    {
        return status is ApplyStatusTraitStatusType.Deter or ApplyStatusTraitStatusType.Stun;
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (IsDeterStatus(StatusType))
        {
            for (var i = 0; i < card.ActiveData.ActiveTraits.Count; i++)
            {
                var activeTrait = card.ActiveData.ActiveTraits[i];
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
            var activeData = active.GetTraitTarget().ActiveData;
            var flag = false;
            for (var i = 0; i < activeData.ActiveTraits.Count; i++)
            {
                activeTrait = activeData.ActiveTraits[i];
                if (activeTrait != active && (activeTrait.GetTraitInfo().IsStatusEffect(ApplyStatusTraitStatusType.Deter, activeTrait) ||
                                              activeTrait.GetTraitInfo().IsStatusEffect(ApplyStatusTraitStatusType.Stun, activeTrait)))
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                for (var j = 0; j < active.GetTraitTarget().ActiveData.ActiveTraits.Count; j++)
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
        if (StatusType == ApplyStatusTraitStatusType.Defender && region == Region.Control)
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

        return (StatusType != ApplyStatusTraitStatusType.Defender || target != Region.Control) &&
               StatusType != ApplyStatusTraitStatusType.Stun && StatusType != ApplyStatusTraitStatusType.Immobilize;
    }

    public override bool CanAttack(CardStack target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return StatusType != ApplyStatusTraitStatusType.Stun;
    }

    public override bool CanCounterAttack(CardStack target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return StatusType != ApplyStatusTraitStatusType.Stun;
    }

    public override bool IsStatusEffect(ApplyStatusTraitStatusType effectId, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        return effectId == StatusType;
    }
}

public enum ApplyStatusTraitStatusType
{
    Stun = 1,
    Deter = 2,
    Defender = 3,
    Immobilize = 4,
    CannotTargetBurn = 5,
    CannotTargetTrait = 6,
    CannotTargetSecret = 7,
    Operative = 8
}