using Frontline.Battle.Traits;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class BaseTrait
{
    public int[] glossaryIds;

    public int traitId;

    public TraitType traitType = TraitType.Passive;

    public bool embarkedInherit;

    public bool hidden;

    public bool functional;

    public TraitActivationReq actRequirement;

    public List<BaseTraitEffect> effects;

    public bool ActivateOnDeploy()
    {
        return traitType == TraitType.Deployed || traitType == TraitType.Passive || traitType == TraitType.Secret ||
               traitType == TraitType.BurnCard || traitType == TraitType.Gear || traitType == TraitType.Basic;
    }

    public bool CanDropAnywhere()
    {
        BaseTraitEffect primaryTargeting = GetPrimaryTargeting(0);
        if (primaryTargeting == null)
        {
            return false;
        }

        if (primaryTargeting.TargetTrait())
        {
            TargetEffect targetEffect = (TargetEffect) primaryTargeting;
            if (targetEffect.dropAnywhere)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool CanActivate(RegionEnum region, sbyte owner)
    {
        BaseTraitEffect primaryTargeting = GetPrimaryTargeting(0);
        if (primaryTargeting == null)
        {
            return false;
        }

        if (primaryTargeting.TargetTrait())
        {
            TargetEffect targetEffect = (TargetEffect) primaryTargeting;
            if (targetEffect.dropAnywhere)
            {
                return true;
            }
        }

        TraitTargeting targets = primaryTargeting.targets;
        if (targets == null)
        {
            return false;
        }

        if (!targets.HasAreaTarget())
        {
            if ((traitType == TraitType.BurnCard || traitType == TraitType.OneShot) &&
                (targets.area == TargetableArea.FriendlyDiscard || targets.area == TargetableArea.EnemyDiscard ||
                 targets.area == TargetableArea.FriendlyHand || targets.area == TargetableArea.EnemyHand ||
                 targets.area == TargetableArea.FriendlyCommander || targets.area == TargetableArea.EnemyCommander))
            {
                return true;
            }

            return false;
        }

        return targets.CheckRegion(region, owner);
    }

    public virtual bool CanActivate(CardStack target, RegionEnum region, sbyte owner)
    {
        BaseTraitEffect primaryTargeting = GetPrimaryTargeting(0);
        if (primaryTargeting == null)
        {
            return false;
        }

        if (primaryTargeting.TargetTrait())
        {
            TargetEffect targetEffect = (TargetEffect) primaryTargeting;
            if (targetEffect.dropAnywhere)
            {
                return true;
            }
        }

        TraitTargeting targets = primaryTargeting.targets;
        if (targets == null)
        {
            return false;
        }

        TargetableArea area = targets.area;
        if (area == TargetableArea.Self)
        {
            return false;
        }

        if (!targets.HasAreaTarget())
        {
            if (target.primaryCard == null)
            {
                return false;
            }

            Card primaryCard = target.primaryCard;
            sbyte owner2 = primaryCard.activeData.owner;
            bool flag = false;
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
                case TargetableArea.BattleFieldNC:
                    if (primaryCard.GetTemplate().Type == CardType.Commander)
                    {
                        return false;
                    }

                    break;
            }

            if (traitType == TraitType.BurnCard && primaryCard.HasStatusEffect(5))
            {
                return false;
            }

            if (traitType == TraitType.OneShot && primaryCard.HasStatusEffect(6))
            {
                return false;
            }

            if (traitType == TraitType.Secret && primaryCard.HasStatusEffect(7))
            {
                return false;
            }

            return true;
        }

        return targets.CheckRegion(region, owner);
    }

    public bool CanActivate(Card target, Card source, RegionEnum region, CCG game)
    {
        List<CardStack> list = game.FindCardStack(target);
        return CanActivate(list[0], region, source.activeData.owner);
    }

    public bool HasActiveTargets(Card card, CardStack target, RegionEnum region, CCG game)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].CheckForAppliedTargets(card, target, region).Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public void Activate(Card card, CardStack target, RegionEnum region, CCG game)
    {
        BaseTraitEffect trigger = GetTrigger(0);
        if (trigger != null)
        {
            trigger.ActivateTrigger(card, target, GetPrimaryTargeting(0).targets);
        }
        else
        {
            sbyte b = 0;
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].priority > b)
                {
                    b = effects[i].priority;
                }
            }

            if (b > 0)
            {
                for (int j = 0; j <= b; j++)
                {
                    for (int k = 0; k < effects.Count; k++)
                    {
                        if (effects[k].priority == j)
                        {
                            effects[k].Activate(card, target, region);
                        }
                    }
                }
            }
            else
            {
                for (int l = 0; l < effects.Count; l++)
                {
                    effects[l].Activate(card, target, region);
                }
            }
        }

        game.PurgeTemporaryEffects();
    }

    public virtual bool CanDeactivate()
    {
        return true;
    }

    public virtual void Deactivate(Card card, Card source)
    {
        ActiveTrait activeTrait = null;
        for (int num = card.activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = card.activeData.activeTraits[num];
            if (activeTrait.traitSourceId == traitId)
            {
                activeTrait.Deactivate(true);
            }
        }
    }

    public virtual bool DisplayTraitDescription()
    {
        return true;
    }

    public BaseTraitEffect GetPrimaryTargeting(sbyte priority)
    {
        BaseTraitEffect baseTraitEffect = null;
        for (int i = 0; i < effects.Count; i++)
        {
            if (baseTraitEffect == null && effects[i].targetPrimary)
            {
                baseTraitEffect = effects[i];
            }

            if (effects[i].TargetTrait() && effects[i].priority == priority)
            {
                return effects[i];
            }
        }

        if (baseTraitEffect == null && effects.Count > 0)
        {
            return effects[0];
        }

        return baseTraitEffect;
    }

    public BaseTraitEffect GetTrigger(int limit)
    {
        BaseTraitEffect baseTraitEffect = null;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].IsTrigger() && effects[i].priority >= limit &&
                (baseTraitEffect == null || effects[i].priority < baseTraitEffect.priority))
            {
                baseTraitEffect = effects[i];
            }
        }

        return baseTraitEffect;
    }
}