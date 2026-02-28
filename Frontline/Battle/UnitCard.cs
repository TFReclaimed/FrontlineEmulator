using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Data.Entities;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class UnitCard : EntityCard
{
    public UnitCard EmbarkedPilot { get; set; }

    public bool PilotEmbarked { get; set; }

    private sbyte attack;

    private sbyte bypassDefense;

    private sbyte defense;

    public UnitCard(CcgGameState game)
        : base(game)
    {
    }

    public UnitCard(CcgGameState game, Card other)
        : base(game, other)
    {
        if (other is UnitCard)
        {
            var unitCard = (UnitCard) other;
            EmbarkedPilot = unitCard.EmbarkedPilot;
            PilotEmbarked = unitCard.PilotEmbarked;
        }
    }

    public UnitCard(CcgGameState game, ItemEntity itemEntity)
        : base(game, itemEntity)
    {
    }

    public override void Setup()
    {
        base.Setup();
        var unitTemplate = (UnitCardTemplate) GetTemplate();
        bypassDefense = 0;
        SetCurrentHealth(unitTemplate.Health);
        SetMaxHealth(unitTemplate.Health);
        SetCurrentDefense(unitTemplate.Defense);
        currentCost = unitTemplate.Cost;
        attack = unitTemplate.Attack;
        defense = unitTemplate.Defense;
        SetCurrentDefense(unitTemplate.Defense);
    }

    public override void InitStackedCards()
    {
        if (EmbarkedPilot != null)
        {
            EmbarkedPilot = (UnitCard) EmbarkedPilot.GenerateAndInit(GameState);
        }

        base.InitStackedCards();
    }

    public override Card FindTraitActor(int cardId, sbyte ownerId)
    {
        var card = base.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        if (EmbarkedPilot == null)
        {
            return null;
        }

        if (EmbarkedPilot.InstanceId == cardId && EmbarkedPilot.ActiveData.Owner == ownerId)
        {
            return EmbarkedPilot;
        }

        return null;
    }

    public override bool DoesMatchTargetingInfo(TraitTargeting info, Card source)
    {
        if (base.DoesMatchTargetingInfo(info, source))
        {
            return true;
        }

        if (HasPilot())
        {
            return EmbarkedPilot.DoesMatchTargetingInfo(info, source);
        }

        return false;
    }

    public override UnitType GetUnitType()
    {
        var unitTemplate = (UnitCardTemplate) GetTemplate();
        return unitTemplate.UnitType;
    }

    public override sbyte GetCurrentAttack(Card target, bool combatLog)
    {
        if (attack == 0)
        {
            return 0;
        }

        var b = attack;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            b2 = activeTrait.GetTraitInfo().GetAttackBonus(target, activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo.Data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            var count = list.Count;
            var targetID = target != null ? target.InstanceId : -1;
            var targetOwner = (sbyte) (target != null ? target.ActiveData.Owner : -1);
            var combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack,
                InstanceId, ActiveData.Owner, targetID, targetOwner);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (var j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public override sbyte GetCurrentBypassDefense(Card target, bool combatLog)
    {
        if (attack == 0)
        {
            return 0;
        }

        var b = bypassDefense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            b2 = activeTrait.GetTraitInfo().GetBypassDefenseBonus(target, activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo.Data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            var count = list.Count;
            var targetID = target != null ? target.InstanceId : -1;
            var targetOwner = (sbyte) (target != null ? target.ActiveData.Owner : -1);
            var combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack,
                InstanceId, ActiveData.Owner, targetID, targetOwner);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (var j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public override sbyte GetCurrentDefense(bool combatLog)
    {
        var activeUnitCardData = (ActiveUnitCardData) ActiveData;
        var b = activeUnitCardData.CurrentDefense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            b2 = activeTrait.GetTraitInfo().GetDefenseBonus(activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo.Data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            var count = list.Count;
            var combatBuffsCCGEvent =
                new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack, InstanceId, ActiveData.Owner, 0, 0);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (var j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public void SetCurrentDefense(sbyte newDefense)
    {
        var activeUnitCardData = (ActiveUnitCardData) ActiveData;
        activeUnitCardData.CurrentDefense = newDefense;
    }

    public void TakeDefenseDamage(sbyte damage)
    {
        var activeUnitCardData = (ActiveUnitCardData) ActiveData;
        activeUnitCardData.CurrentDefense -= damage;
    }

    public sbyte GetMaxDefense()
    {
        return defense;
    }

    public override bool HasActiveTraitEffect(int effectId)
    {
        if (base.HasActiveTraitEffect(effectId))
        {
            return true;
        }

        if (HasPilot())
        {
            return EmbarkedPilot.HasActiveTraitEffect(effectId);
        }

        return false;
    }

    public override bool HasActiveSourceTrait(int traitId)
    {
        if (base.HasActiveSourceTrait(traitId))
        {
            return true;
        }

        if (HasPilot())
        {
            return EmbarkedPilot.HasActiveSourceTrait(traitId);
        }

        return false;
    }

    public override bool CanEmbark()
    {
        if (EmbarkedPilot != null || PilotEmbarked)
        {
            return false;
        }

        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            var traitInfo = ActiveData.ActiveTraits[i].GetTraitInfo();
            if (!traitInfo.CanEmbark())
            {
                return false;
            }
        }

        return true;
    }

    public void EmbarkTraits()
    {
        ActiveData.EmbarkTraits();
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].ActiveData.EmbarkTraits();
        }
    }

    public void DisembarkTraits()
    {
        var hasDeter = IsCardTraitsDetered();
        ActiveData.DisembarkTraits(hasDeter);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].ActiveData.DisembarkTraits(hasDeter);
        }
    }

    public override bool CanDeploy(CardStack target, Region region, bool emptyAvailable, bool embark)
    {
        var cardTemplate = GetTemplate();
        var flag = cardTemplate.CanDeploy(region, ActiveData.Owner);
        if (!flag)
        {
            flag = CanOverrideDeploy(region);
        }

        if (!cardTemplate.CanDeploy(target, emptyAvailable, embark))
        {
            GameState.Logger.Debug("UnitCard.CanDeploy false - Template check failed");
            return false;
        }

        if (embark && target.PrimaryCard != null)
        {
            var primaryCard = target.PrimaryCard;
            if (primaryCard.ActiveData.Owner != ActiveData.Owner)
            {
                GameState.Logger.Debug("UnitCard.CanDeploy false - invalid ebark owner");
                return false;
            }

            if (!CanEmbark() || !primaryCard.CanEmbark())
            {
                GameState.Logger.Debug("UnitCard.CanDeploy false - ineligable ebark unit");
                return false;
            }

            for (var i = 0; i < cardTraits.Length; i++)
            {
                var baseTrait = cardTraits[i];
                if (baseTrait == null)
                {
                    continue;
                }

                for (var j = 0; j < cardTraits[i].Effects.Count; j++)
                {
                    var baseTraitEffect = cardTraits[i].Effects[j];
                    if (!baseTraitEffect.CanEmbark())
                    {
                        GameState.Logger.Debug("UnitCard.CanDeploy false - trait prevents embark " +
                                          baseTraitEffect.EffectTraitId);
                        return false;
                    }
                }
            }
        }
        else if (!flag)
        {
            GameState.Logger.Debug("UnitCard.CanDeploy false - Template check failed");
            return false;
        }

        for (var k = 0; k < cardTraits.Length; k++)
        {
            var baseTrait2 = cardTraits[k];
            if (baseTrait2 == null)
            {
                continue;
            }

            for (var l = 0; l < cardTraits[k].Effects.Count; l++)
            {
                var baseTraitEffect2 = cardTraits[k].Effects[l];
                if (!baseTraitEffect2.CanDeploy(target, region))
                {
                    GameState.Logger.Debug("UnitCard.CanDeploy false - trait prevents deploy " +
                                      baseTraitEffect2.EffectTraitId);
                    return false;
                }
            }
        }

        return true;
    }

    public override bool Deploy(CardStack stack, bool embark, Region target, CardTransitionCcgEvent deployEvent)
    {
        if (base.Deploy(stack, embark, target, deployEvent))
        {
            CheckAndUpdateXp("Deploy");
            return true;
        }

        if (embark)
        {
            var primaryCard = stack.PrimaryCard;
            var type = GetTemplate().Type;
            BaseTrait baseTrait = null;
            var flag = primaryCard.HasAnyActionsAvailable();
            if (type == CardType.Titan && primaryCard.GetTemplate().Type == CardType.Pilot)
            {
                SetActed(15);
                stack.PrimaryCard = this;
                var unitCard = (UnitCard) primaryCard;
                unitCard.PilotEmbarked = true;
                EmbarkedPilot = unitCard;
                if (deployEvent != null)
                {
                    deployEvent.Embark = true;
                    deployEvent.TargetId = unitCard.InstanceId;
                    deployEvent.TargetOwner = unitCard.ActiveData.Owner;
                }

                for (var i = 0; i < cardTraits.Length; i++)
                {
                    baseTrait = cardTraits[i];
                    if (baseTrait != null && baseTrait.ActivateOnDeploy())
                    {
                        baseTrait.Activate(this, stack, target, GameState);
                    }
                }

                GameState.GetPilotEmbarkTrait().Activate(unitCard, stack, target, GameState);
                if (flag)
                {
                    GameState.GetTitanPilotEmbarkTrait().Activate(this, stack, target, GameState);
                }

                CheckAndUpdateXp("Deploy");
                return true;
            }

            if (type == CardType.Pilot && primaryCard.GetTemplate().Type == CardType.Titan)
            {
                SetActed(15);
                var unitCard2 = (UnitCard) primaryCard;
                unitCard2.EmbarkedPilot = this;
                PilotEmbarked = true;
                if (deployEvent != null)
                {
                    deployEvent.Embark = true;
                    deployEvent.TargetId = unitCard2.InstanceId;
                    deployEvent.TargetOwner = unitCard2.ActiveData.Owner;
                }

                for (var j = 0; j < cardTraits.Length; j++)
                {
                    baseTrait = cardTraits[j];
                    if (baseTrait != null && baseTrait.ActivateOnDeploy())
                    {
                        baseTrait.Activate(this, stack, target, GameState);
                    }
                }

                GameState.GetPilotEmbarkTrait().Activate(this, stack, target, GameState);
                if (flag)
                {
                    GameState.GetTitanPilotEmbarkTrait().Activate(unitCard2, stack, target, GameState);
                }

                CheckAndUpdateXp("Deploy");
                return true;
            }

            GameState.Logger.Warning("DEPLOY FAILED - UnitCard.Deploy - invalid embark combo");
        }

        return false;
    }

    public override bool Move(CardStack target, Region region, Region origin, bool embark)
    {
        if (base.Move(target, region, origin, embark))
        {
            return true;
        }

        if (embark)
        {
            var type = GetTemplate().Type;
            var unitCard = (UnitCard) target.PrimaryCard;
            var type2 = unitCard.GetTemplate().Type;
            var flag = unitCard.HasAnyActionsAvailable();
            if (type == CardType.Titan && type2 == CardType.Pilot)
            {
                SetActed(14);
                target.PrimaryCard = this;
                EmbarkedPilot = unitCard;
                unitCard.PilotEmbarked = true;
                EmbarkTraits();
                EmbarkedPilot.EmbarkTraits();
                GameState.GetPilotEmbarkTrait().Activate(EmbarkedPilot, target, region, GameState);
                if (flag)
                {
                    GameState.GetTitanPilotEmbarkTrait().Activate(this, target, region, GameState);
                }

                return true;
            }

            if (type == CardType.Pilot && type2 == CardType.Titan)
            {
                SetActed(14);
                PilotEmbarked = true;
                unitCard.EmbarkedPilot = this;
                EmbarkTraits();
                unitCard.EmbarkTraits();
                GameState.GetPilotEmbarkTrait().Activate(this, target, region, GameState);
                if (flag)
                {
                    GameState.GetTitanPilotEmbarkTrait().Activate(unitCard, target, region, GameState);
                }

                return true;
            }

            GameState.Logger.Warning("MOVE FAILED - UnitCard.Move - invalid embark combo");
        }

        return false;
    }

    public override bool Disembark(CardStack location, Region region)
    {
        SetActed(14);
        EmbarkedPilot = null;
        PilotEmbarked = false;
        ActiveData.MoveTraits(location, region, false);
        return true;
    }

    public override bool HasAttack()
    {
        return attack > 0;
    }

    public override bool CanAttack(CardStack source, CardStack target)
    {
        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            var activeTrait = ActiveData.ActiveTraits[i];
            if (!activeTrait.GetTraitInfo().CanAttack(target, activeTrait))
            {
                GameState.Logger.Debug("UnitCard.CanAttack false - trait prevetns attack " + activeTrait.TraitEffectId);
                return false;
            }
        }

        if (base.CanAttack(source, target) && target.PrimaryCard != null)
        {
            if (ActiveData.Owner != target.PrimaryCard.ActiveData.Owner)
            {
                if (GetTemplate().CanAttack(source, target))
                {
                    if (attack > 0)
                    {
                        return true;
                    }

                    GameState.Logger.Debug("UnitCard.CanAttack false - card base attack is 0");
                }
                else
                {
                    GameState.Logger.Debug("UnitCard.CanAttack false - template check failed");
                }
            }
            else
            {
                GameState.Logger.Debug("UnitCard.CanAttack false - target owner is same as attacker");
            }
        }

        return false;
    }

    public override void Attack(CardStack source, Card target)
    {
        var targetID = target != null ? target.InstanceId : -1;
        var targetOwner = (sbyte) (target != null ? target.ActiveData.Owner : -1);
        CombatCcgEvent combatCCGEvent = null;
        var combatCCGEvent2 = new CombatCcgEvent(CcgEventType.CombatStart, InstanceId, ActiveData.Owner,
            targetID, targetOwner, 0, 0);
        GameState.AddCCGEventLog(combatCCGEvent2);
        if (target != null)
        {
            var traitActorRegion = GameState.GetTraitActorRegion(target.ActiveData.Owner, target.InstanceId);
            ActiveTrait activeTrait = null;
            var list = GameState.FindCardStack(target);
            CardStack cardStack = null;
            if (list.Count > 0)
            {
                cardStack = list[0];
            }

            for (var i = 0; i < cardTraits.Length; i++)
            {
                if (cardTraits[i].TraitType == TraitType.Assault && !ActiveData.TraitActivated[i])
                {
                    ActiveData.TraitActivated[i] = true;
                    cardTraits[i].Activate(this, cardStack, traitActorRegion, GameState);
                }
            }

            sbyte b = 0;
            sbyte bypass = 0;
            if (CanDiscard() || target.CanDiscard())
            {
                SetActed(14);
                ActiveData.AttackTraits(target);
                combatCCGEvent = new CombatCcgEvent(CcgEventType.CombatEnd, InstanceId, ActiveData.Owner, targetID,
                    targetOwner, 0, 0);
                GameState.AddCCGEventLog(combatCCGEvent);
                return;
            }

            var combatCCGEvent3 = new CombatCcgEvent(CcgEventType.CombatAttack, InstanceId, ActiveData.Owner,
                target.InstanceId, target.ActiveData.Owner, b, bypass);
            combatCCGEvent3.Result = 0;
            GameState.AddCCGEventLog(combatCCGEvent3);
            GameState.CardAttacked(this, target);
            b = GetCurrentAttack(target, true);
            bypass = GetCurrentBypassDefense(target, true);
            target.GetCurrentDefense(true);
            target.GetCurrentHealth(true);
            var list2 = new List<EventLogTraitCardInfo>();
            for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
            {
                activeTrait = ActiveData.ActiveTraits[num];
                if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.DamageConvertAp, activeTrait))
                {
                    bypass += b;
                    b = 0;
                    var eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo.Data = 1;
                    list2.Add(eventLogTraitCardInfo);
                    if (activeTrait.HasCharges())
                    {
                        activeTrait.ExpendCharge();
                    }
                }
                else if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.DamageConvertNormal, activeTrait))
                {
                    b += bypass;
                    bypass = 0;
                    var eventLogTraitCardInfo2 = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo2.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo2.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo2.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo2.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo2.Data = 0;
                    list2.Add(eventLogTraitCardInfo2);
                    if (activeTrait.HasCharges())
                    {
                        activeTrait.ExpendCharge();
                    }
                }
            }

            if (list2.Count > 0)
            {
                var combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsConversion,
                    InstanceId, ActiveData.Owner, targetID, targetOwner);
                combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[list2.Count];
                for (var j = 0; j < list2.Count; j++)
                {
                    combatBuffsCCGEvent.BuffTraits[j] = list2[j];
                }

                GameState.AddCCGEventLog(combatBuffsCCGEvent);
            }

            combatCCGEvent3.AttackTotal = b;
            combatCCGEvent3.BypassTotal = bypass;
            if (!CanAttack(source, cardStack) || CanDiscard() || target.CanDiscard())
            {
                SetActed(14);
                ActiveData.AttackTraits(target);
                combatCCGEvent = new CombatCcgEvent(CcgEventType.CombatEnd, InstanceId, ActiveData.Owner, targetID,
                    targetOwner, 0, 0);
                GameState.AddCCGEventLog(combatCCGEvent);
                return;
            }

            if (target.CanCounterAttack(cardStack, source, false))
            {
                combatCCGEvent2.Result = 1;
            }

            if ((combatCCGEvent3.Result = ValidateAttackEffect(this, target)) == 0)
            {
                target.TakeDamage(b, bypass, this, false);
            }
        }

        base.Attack(source, target);
        combatCCGEvent = new CombatCcgEvent(CcgEventType.CombatEnd, InstanceId, ActiveData.Owner, targetID, targetOwner,
            0, 0);
        GameState.AddCCGEventLog(combatCCGEvent);
        CheckForDeathEvent();
        if (target != null)
        {
            target.CheckForDeathEvent();
        }
    }

    public override bool CanCounterAttack(CardStack source, CardStack target, bool inCombat)
    {
        if (GetTemplate().CanAttack(source, target))
        {
            ActiveTrait activeTrait = null;
            for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
            {
                activeTrait = ActiveData.ActiveTraits[i];
                if (!activeTrait.GetTraitInfo().CanCounterAttack(target, activeTrait))
                {
                    if (inCombat && activeTrait.HasCharges())
                    {
                        activeTrait.ExpendCharge();
                    }

                    return false;
                }
            }

            return attack > 0;
        }

        return false;
    }

    public override void CounterAttack(CardStack source, Card target)
    {
        ActiveTrait activeTrait = null;
        sbyte b = 0;
        sbyte bypass = 0;
        var combatCCGEvent = new CombatCcgEvent(CcgEventType.CombatCounter, InstanceId, ActiveData.Owner,
            target.InstanceId, target.ActiveData.Owner, b, bypass);
        combatCCGEvent.Result = 0;
        GameState.AddCCGEventLog(combatCCGEvent);
        GameState.CardCounterAttacked(this, target);
        b = GetCurrentAttack(target, true);
        bypass = GetCurrentBypassDefense(target, true);
        target.GetCurrentDefense(true);
        target.GetCurrentHealth(true);
        var list = new List<EventLogTraitCardInfo>();
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = ActiveData.ActiveTraits[num];
            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.DamageConvertAp, activeTrait))
            {
                bypass += b;
                b = 0;
                var eventLogTraitCardInfo = new EventLogTraitCardInfo();
                eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                eventLogTraitCardInfo.Data = 1;
                list.Add(eventLogTraitCardInfo);
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }
            }
            else if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.DamageConvertNormal, activeTrait))
            {
                b += bypass;
                bypass = 0;
                var eventLogTraitCardInfo2 = new EventLogTraitCardInfo();
                eventLogTraitCardInfo2.InstanceId = activeTrait.GetTraitSource().InstanceId;
                eventLogTraitCardInfo2.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                eventLogTraitCardInfo2.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                eventLogTraitCardInfo2.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                eventLogTraitCardInfo2.Data = 0;
                list.Add(eventLogTraitCardInfo2);
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }
            }
        }

        if (list.Count > 0)
        {
            var targetID = target != null ? target.InstanceId : -1;
            var targetOwner = (sbyte) (target != null ? target.ActiveData.Owner : -1);
            var combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsConversion,
                InstanceId, ActiveData.Owner, targetID, targetOwner);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                combatBuffsCCGEvent.BuffTraits[i] = list[i];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        var b2 = combatCCGEvent.Result = ValidateCounterAttackEffect(this, target);
        combatCCGEvent.AttackTotal = b;
        combatCCGEvent.BypassTotal = bypass;
        if (b2 == 0)
        {
            target.TakeDamage(b, bypass, this, false);
        }

        base.CounterAttack(source, target);
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        var b = attack;
        var bypass2 = bypass;
        ActiveTrait activeTrait = null;
        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            if (activeTrait.GetTraitInfo().IsDamageImmunity(false, activeTrait))
            {
                b = 0;
            }

            if (activeTrait.GetTraitInfo().IsDamageImmunity(true, activeTrait))
            {
                bypass2 = 0;
            }
        }

        var b2 = GetCurrentDefense(false);
        if (b2 < 0)
        {
            b2 = 0;
        }

        var b3 = b < b2 ? b : b2;
        TakeDefenseDamage(b3);
        base.TakeDamage((sbyte) (b - b3), bypass2, source, checkDeath);
    }

    public override void CreateActiveData()
    {
        if (ActiveData == null)
        {
            ActiveData = new ActiveUnitCardData();
            ActiveData.Setup(this);
        }
    }

    public override void InitActiveData()
    {
        if (ActiveData != null)
        {
            base.InitActiveData();
        }

        var unitTemplate = (UnitCardTemplate) GetTemplate();
        bypassDefense = 0;
        attack = unitTemplate.Attack;
        defense = unitTemplate.Defense;
        if (EmbarkedPilot != null)
        {
            EmbarkedPilot.InitActiveData();
        }
    }

    private sbyte ValidateAttackEffect(Card source, Card target)
    {
        ActiveTrait activeTrait = null;
        var flag = false;
        for (var num = source.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = source.ActiveData.ActiveTraits[num];
            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.IgnoreStealth, activeTrait))
            {
                flag = true;
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                break;
            }
        }

        for (var num2 = target.ActiveData.ActiveTraits.Count - 1; num2 >= 0; num2--)
        {
            activeTrait = target.ActiveData.ActiveTraits[num2];
            if (!flag && activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.Stealth, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                return 1;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.Dodge, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                return 3;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.Block, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                return 6;
            }
        }

        return 0;
    }

    public bool IsEmbarked()
    {
        return PilotEmbarked || EmbarkedPilot != null;
    }

    public override bool HasPilot()
    {
        if (template.Type != CardType.Titan)
        {
            return false;
        }

        return EmbarkedPilot != null;
    }

    public override EntityCard GetEmbarkedPilot()
    {
        return EmbarkedPilot;
    }

    public override void CardDeployed(Card deployed)
    {
        base.CardDeployed(deployed);
        if (HasPilot())
        {
            EmbarkedPilot.CardDeployed(deployed);
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        if (HasPilot())
        {
            EmbarkedPilot.NewTurn(playerIndex);
        }
    }

    public override void EndTurn(sbyte playerIndex)
    {
        base.EndTurn(playerIndex);
        ResetDefense();
        if (HasPilot())
        {
            EmbarkedPilot.EndTurn(playerIndex);
        }
    }

    public override void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        base.CardMoved(card, target, region, origin);
        if (HasPilot())
        {
            EmbarkedPilot.CardMoved(card, target, region, origin);
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        if (HasPilot())
        {
            EmbarkedPilot.CardAttacked(attacker, target);
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        if (HasPilot())
        {
            EmbarkedPilot.CardCounterAttacked(attacker, target);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        if (HasPilot())
        {
            EmbarkedPilot.CardGainedStatus(theCard, source, statusType);
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        if (HasPilot())
        {
            EmbarkedPilot.CardDamaged(damagedCard, source);
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        if (HasPilot())
        {
            EmbarkedPilot.CardDied(deadCard, source);
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        if (HasPilot())
        {
            EmbarkedPilot.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        if (HasPilot())
        {
            EmbarkedPilot.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public override void SecretTriggered(Card secret, Card source)
    {
        base.SecretTriggered(secret, source);
        if (HasPilot())
        {
            EmbarkedPilot.SecretTriggered(secret, source);
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        if (HasPilot())
        {
            EmbarkedPilot.SecretDestroyed(secret, source);
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        if (HasPilot())
        {
            EmbarkedPilot.TraitEffectActivating(effect, source, target, region);
        }
    }

    public void CheckAndUpdateXp(string xpTrigger)
    {
        var trigger = RulesetParser.GetXpTrigger(xpTrigger);
        if (trigger <= 0)
        {
            return;
        }

        Xp += trigger;
        var logData = new CardInfoCcgEvent(CcgEventType.CardXpEarned, InstanceId, ActiveData.Owner, trigger, xpTrigger);
        GameState.AddCCGEventLog(logData);
    }

    public override void Discard(Player[] players)
    {
        base.Discard(players);
        if (HasPilot())
        {
            EmbarkedPilot.Discard(players);
        }
    }

    private sbyte ValidateCounterAttackEffect(Card source, Card target)
    {
        ActiveTrait activeTrait = null;
        var flag = false;
        for (var num = source.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = source.ActiveData.ActiveTraits[num];
            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.IgnoreSniper, activeTrait))
            {
                flag = true;
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                break;
            }
        }

        for (var i = 0; i < target.ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = target.ActiveData.ActiveTraits[i];
            if (!flag && activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.Sniper, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                return 4;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.Block, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                return 6;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassive.Dodge, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge();
                }

                return 3;
            }
        }

        return 0;
    }

    private void ResetDefense()
    {
        var activeUnitCardData = (ActiveUnitCardData) ActiveData;
        activeUnitCardData.CurrentDefense = GetMaxDefense();
    }
}