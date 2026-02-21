using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class TraitTrigger : BaseTraitEffect
{
    public TriggerType type;

    public TraitTargeting versusInfo;

    public bool delayActivation;

    public bool allowSameRegion;

    public TraitDurationType activationDelayType;

    public override bool IsTrigger()
    {
        return true;
    }

    public void RunTriggerActivation(Card source, Card target, RegionEnum destination, ActiveTrait active)
    {
        BaseTrait traitTemplate = RulesetParser.GetTraitTemplate(traitParentID);
        sbyte b = 0;
        if (durationData.charges > 0 && active.durationData.charges == 0)
        {
            return;
        }

        active.triggered = false;
        if (active.GetTraitSource() != null && active.GetTraitSource().GetTemplate().Type == CardType.Secret)
        {
            Card traitSource = active.GetTraitSource();
            Card traitTarget = active.GetTraitTarget();
            TraitInfoCCGEvent logData = new TraitInfoCCGEvent(CCGEventType.SecretTriggered, traitParentID,
                effectTraitID, traitTarget.instanceId, traitTarget.activeData.owner, traitSource.instanceId,
                traitSource.activeData.owner, 0);
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

            traitSource.Discard(GameState.players);
            if (flag)
            {
                return;
            }
        }

        for (int j = 0; j < traitTemplate.effects.Count; j++)
        {
            if (traitTemplate.effects[j].priority > b)
            {
                b = traitTemplate.effects[j].priority;
            }
        }

        if (b > 0)
        {
            BaseTraitEffect baseTraitEffect = null;
            for (int k = priority; k < b; k++)
            {
                if (baseTraitEffect != null)
                {
                    break;
                }

                if (k > priority)
                {
                    baseTraitEffect = traitTemplate.GetTrigger(k);
                }

                if (baseTraitEffect != null)
                {
                    ActivateParentEffect(traitTemplate, baseTraitEffect, source, target, destination, active, true);
                    break;
                }

                for (int l = 0; l < traitTemplate.effects.Count; l++)
                {
                    if (traitTemplate.effects[l].priority == k)
                    {
                        ActivateParentEffect(traitTemplate, traitTemplate.effects[l], source, target, destination,
                            active, false);
                    }
                }
            }
        }
        else
        {
            for (int m = 0; m < traitTemplate.effects.Count; m++)
            {
                ActivateParentEffect(traitTemplate, traitTemplate.effects[m], source, target, destination, active,
                    false);
            }
        }

        if (active.HasCharges())
        {
            active.ExpendCharge(GameState);
        }
        else if (durationData.type == TraitDurationType.Instant)
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
            TraitTargeting triggerTarget = parent.GetPrimaryTargeting(trait.priority).targets;
            list = GameState.FindCardStack(traitTarget);
            if (list != null && list.Count > 0)
            {
                target2 = list[0];
            }

            if (trait.targets.scope == TraitTargetScope.TriggeringUnit)
            {
                cardStack = null;
                list = GameState.FindCardStack(source);
                if (list != null && list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.ActivateTrigger(traitTarget, cardStack, triggerTarget);
            }
            else if (trait.targets.scope == TraitTargetScope.TriggerTarget)
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
                GameState.GetTraitActorRegion(traitTarget.activeData.owner, traitTarget.instanceId);
            list = GameState.FindCardStack(traitTarget);
            if (list != null && list.Count > 0)
            {
                target2 = list[0];
            }

            if (trait.targets.scope == TraitTargetScope.TriggeringUnit && source != null)
            {
                cardStack = null;
                list = GameState.FindCardStack(source);
                if (list != null && list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.Activate(traitTarget, cardStack, traitActorRegion);
            }
            else if (trait.targets.scope == TraitTargetScope.TriggerTarget && target != null)
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
                if (trait.targets.scope == TraitTargetScope.TriggeringUnit && source == null)
                {
                    Console.WriteLine("Trigger Activation Error: Trigging Unit is NULL");
                }
                else if (trait.targets.scope == TraitTargetScope.TriggerTarget && target == null)
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
        if (delayActivation && active.triggered)
        {
            sbyte owner = active.GetTraitTarget().activeData.owner;
            if (activationDelayType == TraitDurationType.StartOfTurn)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (activationDelayType == TraitDurationType.StartOfMyTurn && owner == playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (activationDelayType == TraitDurationType.StartOfEnemyTurn && owner != playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
        }

        if (!flag && type == TriggerType.NewTurn)
        {
            if (delayActivation)
            {
                active.triggered = true;
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
        if (delayActivation && active.triggered)
        {
            sbyte owner = active.GetTraitTarget().activeData.owner;
            if (activationDelayType == TraitDurationType.EndOfTurn)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (activationDelayType == TraitDurationType.EndOfMyTurn && owner == playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
            else if (activationDelayType == TraitDurationType.EndOfEnemyTurn && owner != playerIndex)
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
                flag = true;
            }
        }

        if (!flag && type == TriggerType.EndTurn)
        {
            if (delayActivation)
            {
                active.triggered = true;
            }
            else
            {
                RunTriggerActivation(null, null, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardDeployed(Card deployed, ActiveTrait active)
    {
        if (type == TriggerType.Deploy && targets.CardTargetMatch(GameState, deployed, active.GetTraitTarget()))
        {
            int num = deployed.activeData.owner;
            CardStack commander = GameState.players[num].commander;
            if (delayActivation)
            {
                active.triggered = true;
            }
            else
            {
                RunTriggerActivation(commander.primaryCard, deployed, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardMoved(Card theCard, CardStack target, RegionEnum destination, RegionEnum origin,
        ActiveTrait active)
    {
        if (type == TriggerType.Move && targets.CardTargetMatch(GameState, theCard, active.GetTraitTarget()) &&
            versusInfo.CheckRegion(destination, active.GetTraitTarget().activeData.owner) &&
            (destination != origin || allowSameRegion))
        {
            Card primaryCard = target.primaryCard;
            if (delayActivation)
            {
                active.triggered = true;
            }
            else
            {
                RunTriggerActivation(theCard, primaryCard, destination, active);
            }
        }
    }

    public override void CardAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (type == TriggerType.Attack && targets.CardTargetMatch(GameState, attacker, active.GetTraitTarget()) &&
            versusInfo.CardTargetMatch(GameState, target, active.GetTraitTarget()))
        {
            if (delayActivation)
            {
                active.triggered = true;
            }
            else
            {
                RunTriggerActivation(attacker, target, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (type == TriggerType.CounterAttack && targets.CardTargetMatch(GameState, attacker, active.GetTraitTarget()) &&
            versusInfo.CardTargetMatch(GameState, target, active.GetTraitTarget()))
        {
            if (delayActivation)
            {
                active.triggered = true;
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
        if (type == TriggerType.TakeDamage && targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) &&
            versusInfo.CardTargetMatch(GameState, damagedCard, active.GetTraitTarget()))
        {
            if (delayActivation)
            {
                active.triggered = true;
            }
            else
            {
                RunTriggerActivation(source, damagedCard, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        if (type == TriggerType.Destroy && targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) &&
            versusInfo.CardTargetMatch(GameState, deadCard, active.GetTraitTarget()))
        {
            if (delayActivation)
            {
                active.triggered = true;
            }
            else
            {
                RunTriggerActivation(source, deadCard, RegionEnum.NumRegions, active);
            }
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn, ActiveTrait active)
    {
        switch (type)
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

        if (versusInfo.CardTargetMatch(GameState, drawnCard, active.GetTraitTarget()))
        {
            sbyte owner = drawnCard.activeData.owner;
            Card primaryCard = GameState.players[owner].commander.primaryCard;
            if (delayActivation)
            {
                active.triggered = true;
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
        if (type != TriggerType.ActivateDamageEffect && type != TriggerType.ActivateHealEffect)
        {
            return;
        }

        if (type == TriggerType.ActivateDamageEffect)
        {
            if (!effect.IsDamageHeal(true))
            {
                return;
            }
        }
        else if (type == TriggerType.ActivateHealEffect && !effect.IsDamageHeal(false))
        {
            return;
        }

        Card primaryCard = target.primaryCard;
        if (targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) && (primaryCard == null ||
                                                                         versusInfo.CardTargetMatch(GameState, primaryCard,
                                                                             active.GetTraitTarget())))
        {
            if (delayActivation)
            {
                active.triggered = true;
            }
            else
            {
                RunTriggerActivation(source, primaryCard, RegionEnum.NumRegions, active);
            }
        }
    }
}