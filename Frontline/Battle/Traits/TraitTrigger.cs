using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class TraitTrigger : BaseTraitEffect
{
    public TriggerType Type { get; set; }

    public TraitTargeting VersusInfo { get; set; }

    public bool DelayActivation { get; set; }

    public bool AllowSameRegion { get; set; }

    public TraitDurationType ActivationDelayType { get; set; }

    public override bool IsTrigger()
    {
        return true;
    }

    public void RunTriggerActivation(Card source, Card target, RegionEnum destination, ActiveTrait active)
    {
        BaseTrait traitTemplate = RulesetParser.GetTraitTemplate(TraitParentId);
        sbyte b = 0;
        if (DurationData.Charges > 0 && active.DurationData.Charges == 0)
        {
            return;
        }

        active.Triggered = false;
        if (active.GetTraitSource() != null && active.GetTraitSource().GetTemplate().Type == CardType.Secret)
        {
            Card traitSource = active.GetTraitSource();
            Card traitTarget = active.GetTraitTarget();
            TraitInfoCCGEvent logData = new TraitInfoCCGEvent(CCGEventType.SecretTriggered, TraitParentId,
                EffectTraitId, traitTarget.InstanceId, traitTarget.ActiveData.Owner, traitSource.InstanceId,
                traitSource.ActiveData.Owner, 0);
            GameState.AddCCGEventLog(logData);
            GameState.SecretTriggered(traitSource, source);
            bool flag = false;
            ActiveTrait activeTrait = CheckEffectNegation(traitSource);
            if (activeTrait != null)
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                flag = true;
            }

            List<CardStack> list = GameState.FindCardStack(traitSource);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].RemoveCard(traitSource);
            }

            traitSource.Discard(GameState.Players);
            if (flag)
            {
                return;
            }
        }

        for (int j = 0; j < traitTemplate.Effects.Count; j++)
        {
            if (traitTemplate.Effects[j].Priority > b)
            {
                b = traitTemplate.Effects[j].Priority;
            }
        }

        if (b > 0)
        {
            BaseTraitEffect baseTraitEffect = null;
            for (int k = Priority; k < b; k++)
            {
                if (baseTraitEffect != null)
                {
                    break;
                }

                if (k > Priority)
                {
                    baseTraitEffect = traitTemplate.GetTrigger(k);
                }

                if (baseTraitEffect != null)
                {
                    ActivateParentEffect(traitTemplate, baseTraitEffect, source, target, destination, active, true);
                    break;
                }

                for (int l = 0; l < traitTemplate.Effects.Count; l++)
                {
                    if (traitTemplate.Effects[l].Priority == k)
                    {
                        ActivateParentEffect(traitTemplate, traitTemplate.Effects[l], source, target, destination,
                            active, false);
                    }
                }
            }
        }
        else
        {
            for (int m = 0; m < traitTemplate.Effects.Count; m++)
            {
                ActivateParentEffect(traitTemplate, traitTemplate.Effects[m], source, target, destination, active,
                    false);
            }
        }

        if (active.HasCharges())
        {
            active.ExpendCharge(GameState);
        }
        else if (DurationData.Type == TraitDurationType.Instant)
        {
            active.Deactivate(true);
        }

        GameState.PurgeTemporaryEffects();
    }

    private void ActivateParentEffect(BaseTrait parent, BaseTraitEffect trait, Card source, Card target,
        RegionEnum destination, ActiveTrait active, bool secondaryTrigger)
    {
        CardStack target2 = null;
        CardStack cardStack = null;
        List<CardStack> list = null;
        Card traitTarget = active.GetTraitTarget();
        if (secondaryTrigger)
        {
            TraitTargeting triggerTarget = parent.GetPrimaryTargeting(trait.Priority).Targets;
            list = GameState.FindCardStack(traitTarget);
            if (list != null && list.Count > 0)
            {
                target2 = list[0];
            }

            if (trait.Targets.Scope == TraitTargetScope.TriggeringUnit)
            {
                cardStack = null;
                list = GameState.FindCardStack(source);
                if (list != null && list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.ActivateTrigger(traitTarget, cardStack, triggerTarget);
            }
            else if (trait.Targets.Scope == TraitTargetScope.TriggerTarget)
            {
                cardStack = null;
                list = GameState.FindCardStack(target);
                if (list != null && list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.ActivateTrigger(traitTarget, cardStack, triggerTarget);
            }
            else
            {
                trait.ActivateTrigger(traitTarget, target2, triggerTarget);
            }
        }
        else
        {
            if (trait.IsTrigger() || trait.TargetTrait())
            {
                return;
            }

            RegionEnum traitActorRegion =
                GameState.GetTraitActorRegion(traitTarget.ActiveData.Owner, traitTarget.InstanceId);
            list = GameState.FindCardStack(traitTarget);
            if (list != null && list.Count > 0)
            {
                target2 = list[0];
            }

            if (trait.Targets.Scope == TraitTargetScope.TriggeringUnit && source != null)
            {
                cardStack = null;
                list = GameState.FindCardStack(source);
                if (list != null && list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.Activate(traitTarget, cardStack, traitActorRegion);
            }
            else if (trait.Targets.Scope == TraitTargetScope.TriggerTarget && target != null)
            {
                cardStack = null;
                list = GameState.FindCardStack(target);
                if (list != null && list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.Activate(traitTarget, cardStack, destination);
            }
            else
            {
                if (trait.Targets.Scope == TraitTargetScope.TriggeringUnit && source == null)
                {
                    Console.WriteLine("Trigger Activation Error: Trigging Unit is NULL");
                }
                else if (trait.Targets.Scope == TraitTargetScope.TriggerTarget && target == null)
                {
                    Console.WriteLine("Trigger Activation Error: Trigger Target is NULL");
                }

                trait.Activate(traitTarget, target2, traitActorRegion);
            }
        }
    }

    public override bool Embark(ActiveTrait active)
    {
        return base.Embark(active);
    }

    public override bool Disembark(ActiveTrait active)
    {
        return base.Disembark(active);
    }

    public override void NewTurn(ActiveTrait active, sbyte playerIndex)
    {
        bool flag = false;
        if (DelayActivation && active.Triggered)
        {
            sbyte owner = active.GetTraitTarget().ActiveData.Owner;
            if (ActivationDelayType == TraitDurationType.StartOfTurn)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (ActivationDelayType == TraitDurationType.StartOfMyTurn && owner == playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (ActivationDelayType == TraitDurationType.StartOfEnemyTurn && owner != playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
        }

        if (!flag && Type == TriggerType.NewTurn)
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void EndTurn(ActiveTrait active, sbyte playerIndex)
    {
        bool flag = false;
        if (DelayActivation && active.Triggered)
        {
            sbyte owner = active.GetTraitTarget().ActiveData.Owner;
            if (ActivationDelayType == TraitDurationType.EndOfTurn)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (ActivationDelayType == TraitDurationType.EndOfMyTurn && owner == playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (ActivationDelayType == TraitDurationType.EndOfEnemyTurn && owner != playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
        }

        if (!flag && Type == TriggerType.EndTurn)
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardDeployed(Card deployed, ActiveTrait active)
    {
        if (Type == TriggerType.Deploy && Targets.CardTargetMatch(GameState, deployed, active.GetTraitTarget()))
        {
            int num = deployed.ActiveData.Owner;
            CardStack commander = GameState.Players[num].Commander;
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(commander.PrimaryCard, deployed, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardMoved(Card theCard, CardStack target, RegionEnum destination, RegionEnum origin,
        ActiveTrait active)
    {
        if (Type == TriggerType.Move && Targets.CardTargetMatch(GameState, theCard, active.GetTraitTarget()) &&
            VersusInfo.CheckRegion(destination, active.GetTraitTarget().ActiveData.Owner) &&
            (destination != origin || AllowSameRegion))
        {
            Card primaryCard = target.PrimaryCard;
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(theCard, primaryCard, destination, active);
            }
        }
    }

    public override void CardAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (Type == TriggerType.Attack && Targets.CardTargetMatch(GameState, attacker, active.GetTraitTarget()) &&
            VersusInfo.CardTargetMatch(GameState, target, active.GetTraitTarget()))
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(attacker, target, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (Type == TriggerType.CounterAttack && Targets.CardTargetMatch(GameState, attacker, active.GetTraitTarget()) &&
            VersusInfo.CardTargetMatch(GameState, target, active.GetTraitTarget()))
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(attacker, target, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardActivateTrait(Card source, Card target, ActiveTrait active)
    {
    }

    public override void CardHacked(Card runner, Card target, ActiveTrait active)
    {
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType, ActiveTrait activeTrait)
    {
    }

    public override void CardDamaged(Card damagedCard, Card source, ActiveTrait active)
    {
        if (Type == TriggerType.TakeDamage && Targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) &&
            VersusInfo.CardTargetMatch(GameState, damagedCard, active.GetTraitTarget()))
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(source, damagedCard, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        if (Type == TriggerType.Destroy && Targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) &&
            VersusInfo.CardTargetMatch(GameState, deadCard, active.GetTraitTarget()))
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(source, deadCard, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn, ActiveTrait active)
    {
        switch (Type)
        {
            default:
                return;
            case TriggerType.DeckDraw:
                if (!regularDraw)
                {
                    return;
                }

                break;
            case TriggerType.BonusDeckDraw:
                if (!regularDraw || isNewTurn)
                {
                    return;
                }

                break;
            case TriggerType.SupportDraw:
                if (regularDraw)
                {
                    return;
                }

                break;
            case TriggerType.BonusSupportDraw:
                if (regularDraw || isNewTurn)
                {
                    return;
                }

                break;
        }

        if (VersusInfo.CardTargetMatch(GameState, drawnCard, active.GetTraitTarget()))
        {
            sbyte owner = drawnCard.ActiveData.Owner;
            Card primaryCard = GameState.Players[owner].Commander.PrimaryCard;
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(primaryCard, drawnCard, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void SecretTriggered(Card secret, Card source, ActiveTrait active)
    {
    }

    public override void SecretDestroyed(Card secret, Card source, ActiveTrait active)
    {
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region,
        ActiveTrait active)
    {
        if (Type != TriggerType.ActivateDamageEffect && Type != TriggerType.ActivateHealEffect)
        {
            return;
        }

        if (Type == TriggerType.ActivateDamageEffect)
        {
            if (!effect.IsDamageHeal(true))
            {
                return;
            }
        }
        else if (Type == TriggerType.ActivateHealEffect && !effect.IsDamageHeal(false))
        {
            return;
        }

        Card primaryCard = target.PrimaryCard;
        if (Targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) && (primaryCard == null ||
                                                                         VersusInfo.CardTargetMatch(GameState, primaryCard,
                                                                             active.GetTraitTarget())))
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(source, primaryCard, RegionEnum.NumRegions, active);
            }
        }
    }
}