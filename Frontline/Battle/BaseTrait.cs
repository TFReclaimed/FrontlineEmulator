using Frontline.Battle.Traits;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class BaseTrait
{
    public int[] GlossaryIds { get; set; }

    public int TraitId { get; set; }

    public TraitType TraitType { get; set; } = TraitType.Passive;

    public bool EmbarkedInherit { get; set; }

    public bool Hidden { get; set; }

    public bool Functional { get; set; }

    public TraitActivationReq ActRequirement { get; set; }

    public List<BaseTraitEffect> Effects { get; set; }

    public void Init(CcgGameState gameState)
    {
        foreach (var effect in Effects)
        {
            effect.Init(gameState);
        }
    }

    public bool ActivateOnDeploy()
    {
        return TraitType == TraitType.Deployed || TraitType == TraitType.Passive || TraitType == TraitType.Secret ||
               TraitType == TraitType.BurnCard || TraitType == TraitType.Gear || TraitType == TraitType.Basic;
    }

    public virtual bool CanActivate(Region region, sbyte owner)
    {
        var primaryTargeting = GetPrimaryTargeting(0);
        if (primaryTargeting == null)
        {
            return false;
        }

        if (primaryTargeting.TargetTrait())
        {
            var targetEffect = (TargetEffect) primaryTargeting;
            if (targetEffect.DropAnywhere)
            {
                return true;
            }
        }

        var targets = primaryTargeting.Targets;
        if (targets == null)
        {
            return false;
        }

        if (!targets.HasAreaTarget())
        {
            if ((TraitType == TraitType.BurnCard || TraitType == TraitType.OneShot) &&
                (targets.Area == TargetableArea.FriendlyDiscard || targets.Area == TargetableArea.EnemyDiscard ||
                 targets.Area == TargetableArea.FriendlyHand || targets.Area == TargetableArea.EnemyHand ||
                 targets.Area == TargetableArea.FriendlyCommander || targets.Area == TargetableArea.EnemyCommander))
            {
                return true;
            }

            return false;
        }

        return targets.CheckRegion(region, owner);
    }

    public virtual bool CanActivate(CardStack target, Region region, sbyte owner)
    {
        var primaryTargeting = GetPrimaryTargeting(0);
        if (primaryTargeting == null)
        {
            return false;
        }

        if (primaryTargeting.TargetTrait())
        {
            var targetEffect = (TargetEffect) primaryTargeting;
            if (targetEffect.DropAnywhere)
            {
                return true;
            }
        }

        var targets = primaryTargeting.Targets;
        if (targets == null)
        {
            return false;
        }

        var area = targets.Area;
        if (area == TargetableArea.Self)
        {
            return false;
        }

        if (!targets.HasAreaTarget())
        {
            if (target.PrimaryCard == null)
            {
                return false;
            }

            var primaryCard = target.PrimaryCard;
            var owner2 = primaryCard.ActiveData.Owner;
            var flag = false;
            if (targets.CheckFriendly() && owner2 == owner)
            {
                flag = true;
            }

            if (targets.CheckEnemy() && owner2 != owner)
            {
                flag = true;
            }

            if (!flag)
            {
                return false;
            }

            if (!targets.DoesMatchType(primaryCard))
            {
                return false;
            }

            if (!targets.CheckRegion(region, owner))
            {
                return false;
            }

            switch (area)
            {
                case TargetableArea.AnyCommander:
                case TargetableArea.FriendlyCommander:
                case TargetableArea.EnemyCommander:
                    if (primaryCard.GetTemplate().Type != CardType.Commander)
                    {
                        return false;
                    }

                    break;
                case TargetableArea.BattleFieldNc:
                    if (primaryCard.GetTemplate().Type == CardType.Commander)
                    {
                        return false;
                    }

                    break;
            }

            if (TraitType == TraitType.BurnCard && primaryCard.HasStatusEffect(5))
            {
                return false;
            }

            if (TraitType == TraitType.OneShot && primaryCard.HasStatusEffect(6))
            {
                return false;
            }

            if (TraitType == TraitType.Secret && primaryCard.HasStatusEffect(7))
            {
                return false;
            }

            return true;
        }

        return targets.CheckRegion(region, owner);
    }

    public bool CanActivate(Card target, Card source, Region region, CcgGameState game)
    {
        var list = game.FindCardStack(target);
        return CanActivate(list[0], region, source.ActiveData.Owner);
    }

    public bool HasActiveTargets(Card card, CardStack target, Region region, CcgGameState game)
    {
        for (var i = 0; i < Effects.Count; i++)
        {
            if (Effects[i].CheckForAppliedTargets(card, target, region).Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public void Activate(Card card, CardStack target, Region region, CcgGameState game)
    {
        var trigger = GetTrigger(0);
        if (trigger != null)
        {
            trigger.ActivateTrigger(card, target, GetPrimaryTargeting(0).Targets);
        }
        else
        {
            sbyte b = 0;
            for (var i = 0; i < Effects.Count; i++)
            {
                if (Effects[i].Priority > b)
                {
                    b = Effects[i].Priority;
                }
            }

            if (b > 0)
            {
                for (var j = 0; j <= b; j++)
                {
                    for (var k = 0; k < Effects.Count; k++)
                    {
                        if (Effects[k].Priority == j)
                        {
                            Effects[k].Activate(card, target, region);
                        }
                    }
                }
            }
            else
            {
                for (var l = 0; l < Effects.Count; l++)
                {
                    Effects[l].Activate(card, target, region);
                }
            }
        }

        game.PurgeTemporaryEffects();
    }

    public virtual void Deactivate(Card card, Card source)
    {
        ActiveTrait activeTrait = null;
        for (var num = card.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = card.ActiveData.ActiveTraits[num];
            if (activeTrait.TraitSourceId == TraitId)
            {
                activeTrait.Deactivate(true);
            }
        }
    }

    public BaseTraitEffect GetPrimaryTargeting(sbyte priority)
    {
        BaseTraitEffect baseTraitEffect = null;
        for (var i = 0; i < Effects.Count; i++)
        {
            if (baseTraitEffect == null && Effects[i].TargetPrimary)
            {
                baseTraitEffect = Effects[i];
            }

            if (Effects[i].TargetTrait() && Effects[i].Priority == priority)
            {
                return Effects[i];
            }
        }

        if (baseTraitEffect == null && Effects.Count > 0)
        {
            return Effects[0];
        }

        return baseTraitEffect;
    }

    public BaseTraitEffect GetTrigger(int limit)
    {
        BaseTraitEffect baseTraitEffect = null;
        for (var i = 0; i < Effects.Count; i++)
        {
            if (Effects[i].IsTrigger() && Effects[i].Priority >= limit &&
                (baseTraitEffect == null || Effects[i].Priority < baseTraitEffect.Priority))
            {
                baseTraitEffect = Effects[i];
            }
        }

        return baseTraitEffect;
    }
}