using Frontline.Battle.CcgEvents;
using Frontline.Battle.Data;
using Frontline.Battle.Data.Card;

namespace Frontline.Battle.Traits;

public class TraitTrigger : BaseTraitEffect
{
    public TriggerType Type { get; set; }

    public required TraitTargeting VersusInfo { get; set; }

    public bool DelayActivation { get; set; }

    public bool AllowSameRegion { get; set; }

    public TraitDurationType ActivationDelayType { get; set; }

    public override bool IsTrigger()
    {
        return true;
    }

    public void RunTriggerActivation(Card? source, Card? target, Region destination, ActiveTrait active)
    {
        var traitTemplate = RulesetParser.GetTraitTemplate(TraitParentId);
        if (traitTemplate == null)
        {
            GameState.Logger.Warning("Unable to find trait template for TraitParentId {TraitParentId}", TraitParentId);
            return;
        }

        if (DurationData.Charges > 0 && active.DurationData.Charges == 0)
        {
            return;
        }

        active.Triggered = false;
        if (active.GetTraitSource() != null && active.GetTraitSource().GetTemplate().Type == CardType.Secret)
        {
            var traitSource = active.GetTraitSource();
            var traitTarget = active.GetTraitTarget();
            var secretEvent = new TraitInfoCcgEvent(CcgEventType.SecretTriggered, TraitParentId,
                EffectTraitId, traitTarget.InstanceId, traitTarget.ActiveData.Owner, traitSource.InstanceId,
                traitSource.ActiveData.Owner, 0);
            GameState.AddCcgEventLog(secretEvent);
            GameState.SecretTriggered(traitSource, source);
            var flag = false;
            var activeTrait = CheckEffectNegation(traitSource);
            if (activeTrait != null)
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                flag = true;
            }

            var list = GameState.FindCardStack(traitSource);
            foreach (var cardStack in list)
            {
                cardStack.RemoveCard(traitSource);
            }

            traitSource.Discard(GameState.Players);
            if (flag)
            {
                return;
            }
        }

        sbyte priority = 0;
        foreach (var effect in traitTemplate.Effects)
        {
            if (effect.Priority > priority)
            {
                priority = effect.Priority;
            }
        }

        if (priority > 0)
        {
            BaseTraitEffect? baseTraitEffect = null;
            for (int k = Priority; k < priority; k++)
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

                foreach (var effect in traitTemplate.Effects)
                {
                    if (effect.Priority == k)
                    {
                        ActivateParentEffect(traitTemplate, effect, source, target, destination,
                            active, false);
                    }
                }
            }
        }
        else
        {
            foreach (var effect in traitTemplate.Effects)
            {
                ActivateParentEffect(traitTemplate, effect, source, target, destination, active, false);
            }
        }

        if (active.HasCharges())
        {
            active.ExpendCharge();
        }
        else if (DurationData.Type == TraitDurationType.Instant)
        {
            active.Deactivate(true);
        }

        GameState.PurgeTemporaryEffects();
    }

    private void ActivateParentEffect(BaseTrait parent, BaseTraitEffect trait, Card? source, Card? target,
        Region destination, ActiveTrait active, bool secondaryTrigger)
    {
        CardStack? target2 = null;
        CardStack? cardStack;
        List<CardStack> list;
        var traitTarget = active.GetTraitTarget();
        if (secondaryTrigger)
        {
            var triggerTarget = parent.GetPrimaryTargeting(trait.Priority)!.Targets;
            list = GameState.FindCardStack(traitTarget);
            if (list.Count > 0)
            {
                target2 = list[0];
            }

            if (trait.Targets.Scope == TraitTargetScope.TriggeringUnit)
            {
                cardStack = null;
                list = GameState.FindCardStack(source!);
                if (list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.ActivateTrigger(traitTarget, cardStack, triggerTarget);
            }
            else if (trait.Targets.Scope == TraitTargetScope.TriggerTarget)
            {
                cardStack = null;
                list = GameState.FindCardStack(target!);
                if (list.Count > 0)
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

            var traitActorRegion = GameState.GetTraitActorRegion(traitTarget.ActiveData.Owner, traitTarget.InstanceId);
            list = GameState.FindCardStack(traitTarget);
            target2 = list[0];

            if (trait.Targets.Scope == TraitTargetScope.TriggeringUnit && source != null)
            {
                cardStack = null;
                list = GameState.FindCardStack(source);
                if (list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.Activate(traitTarget, cardStack, traitActorRegion);
            }
            else if (trait.Targets.Scope == TraitTargetScope.TriggerTarget && target != null)
            {
                cardStack = null;
                list = GameState.FindCardStack(target);
                if (list.Count > 0)
                {
                    cardStack = list[0];
                }

                trait.Activate(traitTarget, cardStack, destination);
            }
            else
            {
                if (trait.Targets.Scope == TraitTargetScope.TriggeringUnit && source == null)
                {
                    GameState.Logger.Warning("Trigger activation error: Triggering unit is NULL");
                }
                else if (trait.Targets.Scope == TraitTargetScope.TriggerTarget && target == null)
                {
                    GameState.Logger.Warning("Trigger activation error: Trigger target is NULL");
                }

                trait.Activate(traitTarget, target2, traitActorRegion);
            }
        }
    }

    public override void NewTurn(ActiveTrait active, sbyte playerIndex)
    {
        var triggered = false;
        if (DelayActivation && active.Triggered)
        {
            var owner = active.GetTraitTarget().ActiveData.Owner;
            if (ActivationDelayType == TraitDurationType.StartOfTurn)
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
                triggered = true;
            }
            else if (ActivationDelayType == TraitDurationType.StartOfMyTurn && owner == playerIndex)
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
                triggered = true;
            }
            else if (ActivationDelayType == TraitDurationType.StartOfEnemyTurn && owner != playerIndex)
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
                triggered = true;
            }
        }

        if (!triggered && Type == TriggerType.NewTurn)
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
            }
        }
    }

    public override void EndTurn(ActiveTrait active, sbyte playerIndex)
    {
        var triggered = false;
        if (DelayActivation && active.Triggered)
        {
            var owner = active.GetTraitTarget().ActiveData.Owner;
            if (ActivationDelayType == TraitDurationType.EndOfTurn)
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
                triggered = true;
            }
            else if (ActivationDelayType == TraitDurationType.EndOfMyTurn && owner == playerIndex)
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
                triggered = true;
            }
            else if (ActivationDelayType == TraitDurationType.EndOfEnemyTurn && owner != playerIndex)
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
                triggered = true;
            }
        }

        if (!triggered && Type == TriggerType.EndTurn)
        {
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(null, null, Region.NumRegions, active);
            }
        }
    }

    public override void CardDeployed(Card deployed, ActiveTrait active)
    {
        if (Type != TriggerType.Deploy || !Targets.CardTargetMatch(GameState, deployed, active.GetTraitTarget()))
        {
            return;
        }

        int owner = deployed.ActiveData.Owner;
        var commander = GameState.Players[owner].Commander;
        if (DelayActivation)
        {
            active.Triggered = true;
        }
        else
        {
            RunTriggerActivation(commander.PrimaryCard, deployed, Region.NumRegions, active);
        }
    }

    public override void CardMoved(Card theCard, CardStack target, Region destination, Region origin,
        ActiveTrait active)
    {
        if (Type != TriggerType.Move || !Targets.CardTargetMatch(GameState, theCard, active.GetTraitTarget()) ||
            !VersusInfo.CheckRegion(destination, active.GetTraitTarget().ActiveData.Owner) ||
            (destination == origin && !AllowSameRegion))
        {
            return;
        }

        var primaryCard = target.PrimaryCard;
        if (DelayActivation)
        {
            active.Triggered = true;
        }
        else
        {
            RunTriggerActivation(theCard, primaryCard, destination, active);
        }
    }

    public override void CardAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (Type != TriggerType.Attack || !Targets.CardTargetMatch(GameState, attacker, active.GetTraitTarget()) ||
            !VersusInfo.CardTargetMatch(GameState, target, active.GetTraitTarget()))
        {
            return;
        }

        if (DelayActivation)
        {
            active.Triggered = true;
        }
        else
        {
            RunTriggerActivation(attacker, target, Region.NumRegions, active);
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target, ActiveTrait active)
    {
        if (Type != TriggerType.CounterAttack ||
            !Targets.CardTargetMatch(GameState, attacker, active.GetTraitTarget()) ||
            !VersusInfo.CardTargetMatch(GameState, target, active.GetTraitTarget()))
        {
            return;
        }

        if (DelayActivation)
        {
            active.Triggered = true;
        }
        else
        {
            RunTriggerActivation(attacker, target, Region.NumRegions, active);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType, ActiveTrait activeTrait)
    {
    }

    public override void CardDamaged(Card damagedCard, Card source, ActiveTrait active)
    {
        if (Type != TriggerType.TakeDamage || !Targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) ||
            !VersusInfo.CardTargetMatch(GameState, damagedCard, active.GetTraitTarget()))
        {
            return;
        }

        if (DelayActivation)
        {
            active.Triggered = true;
        }
        else
        {
            RunTriggerActivation(source, damagedCard, Region.NumRegions, active);
        }
    }

    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        if (Type != TriggerType.Destroy || !Targets.CardTargetMatch(GameState, source, active.GetTraitTarget()) ||
            !VersusInfo.CardTargetMatch(GameState, deadCard, active.GetTraitTarget()))
        {
            return;
        }

        if (DelayActivation)
        {
            active.Triggered = true;
        }
        else
        {
            RunTriggerActivation(source, deadCard, Region.NumRegions, active);
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
            var owner = drawnCard.ActiveData.Owner;
            var primaryCard = GameState.Players[owner].Commander.PrimaryCard;
            if (DelayActivation)
            {
                active.Triggered = true;
            }
            else
            {
                RunTriggerActivation(primaryCard, drawnCard, Region.NumRegions, active);
            }
        }
    }

    public override void SecretTriggered(Card secret, Card? source, ActiveTrait active)
    {
    }

    public override void SecretDestroyed(Card secret, Card source, ActiveTrait active)
    {
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack? target, Region region,
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

        var primaryCard = target!.PrimaryCard;
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
                RunTriggerActivation(source, primaryCard, Region.NumRegions, active);
            }
        }
    }
}