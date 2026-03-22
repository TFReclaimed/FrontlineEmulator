using Frontline.Battle.Data.Card;
using Frontline.Battle.Traits;

namespace Frontline.Battle;

public class BaseTrait
{
    public int TraitId { get; set; }

    public TraitType TraitType { get; set; } = TraitType.Passive;

    public List<BaseTraitEffect> Effects { get; set; } = [];

    public void Init(CcgGameState gameState)
    {
        foreach (var effect in Effects)
        {
            effect.Init(gameState);
        }
    }

    public bool ActivateOnDeploy()
    {
        return TraitType is TraitType.Deployed or TraitType.Passive || TraitType == TraitType.Secret ||
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

        if (targets.HasAreaTarget())
        {
            return targets.CheckRegion(region, owner);
        }

        return (TraitType == TraitType.BurnCard || TraitType == TraitType.OneShot) &&
               (targets.Area == TargetableArea.FriendlyDiscard || targets.Area == TargetableArea.EnemyDiscard ||
                targets.Area == TargetableArea.FriendlyHand || targets.Area == TargetableArea.EnemyHand ||
                targets.Area == TargetableArea.FriendlyCommander || targets.Area == TargetableArea.EnemyCommander);
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

        var area = targets.Area;
        if (area == TargetableArea.Self)
        {
            return false;
        }

        if (targets.HasAreaTarget())
        {
            return targets.CheckRegion(region, owner);
        }

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

        if (TraitType == TraitType.BurnCard && primaryCard.HasStatusEffect(ApplyStatusTraitStatusType.CannotTargetBurn))
        {
            return false;
        }

        if (TraitType == TraitType.OneShot && primaryCard.HasStatusEffect(ApplyStatusTraitStatusType.CannotTargetTrait))
        {
            return false;
        }

        if (TraitType == TraitType.Secret && primaryCard.HasStatusEffect(ApplyStatusTraitStatusType.CannotTargetSecret))
        {
            return false;
        }

        return true;

    }

    public bool CanActivate(Card target, Card source, Region region, CcgGameState game)
    {
        var list = game.FindCardStack(target);
        return CanActivate(list[0], region, source.ActiveData.Owner);
    }

    public bool HasActiveTargets(Card card, CardStack? target, Region region, CcgGameState game)
    {
        foreach (var effect in Effects)
        {
            if (effect.CheckForAppliedTargets(card, target, region).Count > 0)
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
            trigger.ActivateTrigger(card, target, GetPrimaryTargeting(0)!.Targets);
        }
        else
        {
            sbyte priority = 0;
            foreach (var effect in Effects)
            {
                if (effect.Priority > priority)
                {
                    priority = effect.Priority;
                }
            }

            if (priority > 0)
            {
                for (var j = 0; j <= priority; j++)
                {
                    foreach (var effect in Effects)
                    {
                        if (effect.Priority == j)
                        {
                            effect.Activate(card, target, region);
                        }
                    }
                }
            }
            else
            {
                foreach (var effect in Effects)
                {
                    effect.Activate(card, target, region);
                }
            }
        }

        game.PurgeTemporaryEffects();
    }

    public virtual void Deactivate(Card card, Card source)
    {
        for (var num = card.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = card.ActiveData.ActiveTraits[num];
            if (activeTrait.TraitSourceId == TraitId)
            {
                activeTrait.Deactivate(true);
            }
        }
    }

    public BaseTraitEffect? GetPrimaryTargeting(sbyte priority)
    {
        BaseTraitEffect? baseTraitEffect = null;
        foreach (var effect in Effects)
        {
            if (baseTraitEffect == null && effect.TargetPrimary)
            {
                baseTraitEffect = effect;
            }

            if (effect.TargetTrait() && effect.Priority == priority)
            {
                return effect;
            }
        }

        if (baseTraitEffect == null && Effects.Count > 0)
        {
            return Effects[0];
        }

        return baseTraitEffect;
    }

    public BaseTraitEffect? GetTrigger(int limit)
    {
        BaseTraitEffect? baseTraitEffect = null;
        foreach (var effect in Effects)
        {
            if (effect.IsTrigger() && effect.Priority >= limit &&
                (baseTraitEffect == null || effect.Priority < baseTraitEffect.Priority))
            {
                baseTraitEffect = effect;
            }
        }

        return baseTraitEffect;
    }
}