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
            foreach (var activeTrait in card.ActiveData.ActiveTraits)
            {
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
            var activeData = active.GetTraitTarget().ActiveData;
            var flag = false;
            foreach (var trait in activeData.ActiveTraits)
            {
                if (trait != active && (trait.GetTraitInfo().IsStatusEffect(ApplyStatusTraitStatusType.Deter, trait) ||
                                        trait.GetTraitInfo().IsStatusEffect(ApplyStatusTraitStatusType.Stun, trait)))
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                foreach (var trait in active.GetTraitTarget().ActiveData.ActiveTraits)
                {
                    trait.Detered = !trait.EmbarkedCheck();
                }

                active.GetTraitTarget().OnRemovedDeter();
            }
        }

        base.Deactivate(active);
    }

    public override bool CanDeploy(CardStack target, Region region)
    {
        return StatusType != ApplyStatusTraitStatusType.Defender || region != Region.Control;
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