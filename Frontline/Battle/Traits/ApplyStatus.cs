namespace Frontline.Battle.Traits;

public class ApplyStatus : BaseTraitEffect
{
    public const sbyte Stun = 1;

    public const sbyte Deter = 2;

    public const sbyte Defender = 3;

    public const sbyte Immobilize = 4;

    public sbyte StatusType { get; set; }

    public static bool IsDeterStatus(sbyte status)
    {
        return status is Deter or Stun;
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
                if (activeTrait != active && (activeTrait.GetTraitInfo().IsStatusEffect(2, activeTrait) ||
                                              activeTrait.GetTraitInfo().IsStatusEffect(1, activeTrait)))
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
        if (StatusType == Defender && region == Region.Control)
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

        return (StatusType != Defender || target != Region.Control) && StatusType != Stun && StatusType != Immobilize;
    }

    public override bool CanAttack(CardStack target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return StatusType != Stun;
    }

    public override bool CanCounterAttack(CardStack target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return true;
        }

        return StatusType != Stun;
    }

    public override bool IsStatusEffect(sbyte effectId, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        return effectId == StatusType;
    }
}