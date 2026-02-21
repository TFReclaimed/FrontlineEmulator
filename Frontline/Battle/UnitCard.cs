using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class UnitCard : EntityCard
{
    public UnitCard embarkedPilot;

    public bool pilotEmbarked;

    private sbyte attack;

    private sbyte bypassDefense;

    private sbyte defense;

    public UnitCard(CCG game)
        : base(game)
    {
    }

    public UnitCard(CCG game, Card other)
        : base(game, other)
    {
        if (other is UnitCard)
        {
            UnitCard unitCard = (UnitCard) other;
            embarkedPilot = unitCard.embarkedPilot;
            pilotEmbarked = unitCard.pilotEmbarked;
        }
    }

    public override void Setup()
    {
        base.Setup();
        UnitCardTemplate unitTemplate = (UnitCardTemplate) GetTemplate();
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
        if (embarkedPilot != null)
        {
            embarkedPilot = (UnitCard) embarkedPilot.GenerateAndInit(GameState);
        }

        base.InitStackedCards();
    }

    public override Card FindTraitActor(int cardId, sbyte ownerId)
    {
        Card card = base.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        if (embarkedPilot == null)
        {
            return null;
        }

        if (embarkedPilot.instanceId == cardId && embarkedPilot.activeData.owner == ownerId)
        {
            return embarkedPilot;
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
            return embarkedPilot.DoesMatchTargetingInfo(info, source);
        }

        return false;
    }

    public override UnitType GetUnitType()
    {
        UnitCardTemplate unitTemplate = (UnitCardTemplate) GetTemplate();
        return unitTemplate.UnitType;
    }

    public override sbyte GetCurrentAttack(Card target, bool combatLog)
    {
        if (attack == 0)
        {
            return 0;
        }

        sbyte b = attack;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b2 = activeTrait.GetTraitInfo().GetAttackBonus(target, activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.instanceId = activeTrait.GetTraitSource().instanceId;
                    eventLogTraitCardInfo.owner = activeTrait.GetTraitSource().activeData.owner;
                    eventLogTraitCardInfo.effectID = activeTrait.GetTraitInfo().effectTraitID;
                    eventLogTraitCardInfo.traitID = activeTrait.GetTraitInfo().traitParentID;
                    eventLogTraitCardInfo.data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            int count = list.Count;
            int targetID = ((target != null) ? target.instanceId : (-1));
            sbyte targetOwner = (sbyte) ((target != null) ? target.activeData.owner : (-1));
            CombatBuffsCCGEvent combatBuffsCCGEvent = new CombatBuffsCCGEvent(CCGEventType.CombatBuffsAttack,
                instanceId, activeData.owner, targetID, targetOwner);
            combatBuffsCCGEvent.buffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.buffTraits[j] = list[j];
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

        sbyte b = bypassDefense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b2 = activeTrait.GetTraitInfo().GetBypassDefenseBonus(target, activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.instanceId = activeTrait.GetTraitSource().instanceId;
                    eventLogTraitCardInfo.owner = activeTrait.GetTraitSource().activeData.owner;
                    eventLogTraitCardInfo.effectID = activeTrait.GetTraitInfo().effectTraitID;
                    eventLogTraitCardInfo.traitID = activeTrait.GetTraitInfo().traitParentID;
                    eventLogTraitCardInfo.data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            int count = list.Count;
            int targetID = ((target != null) ? target.instanceId : (-1));
            sbyte targetOwner = (sbyte) ((target != null) ? target.activeData.owner : (-1));
            CombatBuffsCCGEvent combatBuffsCCGEvent = new CombatBuffsCCGEvent(CCGEventType.CombatBuffsAttack,
                instanceId, activeData.owner, targetID, targetOwner);
            combatBuffsCCGEvent.buffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.buffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public override sbyte GetCurrentDefense(bool combatLog)
    {
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) activeData;
        sbyte b = activeUnitCardData.currentDefense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b2 = activeTrait.GetTraitInfo().GetDefenseBonus(activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.instanceId = activeTrait.GetTraitSource().instanceId;
                    eventLogTraitCardInfo.owner = activeTrait.GetTraitSource().activeData.owner;
                    eventLogTraitCardInfo.effectID = activeTrait.GetTraitInfo().effectTraitID;
                    eventLogTraitCardInfo.traitID = activeTrait.GetTraitInfo().traitParentID;
                    eventLogTraitCardInfo.data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            int count = list.Count;
            CombatBuffsCCGEvent combatBuffsCCGEvent =
                new CombatBuffsCCGEvent(CCGEventType.CombatBuffsAttack, instanceId, activeData.owner, 0, 0);
            combatBuffsCCGEvent.buffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.buffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public void SetCurrentDefense(sbyte newDefense)
    {
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) activeData;
        activeUnitCardData.currentDefense = newDefense;
    }

    public void TakeDefenseDamage(sbyte damage)
    {
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) activeData;
        activeUnitCardData.currentDefense -= damage;
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
            return embarkedPilot.HasActiveTraitEffect(effectId);
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
            return embarkedPilot.HasActiveSourceTrait(traitId);
        }

        return false;
    }

    public override bool CanEmbark()
    {
        if (embarkedPilot != null || pilotEmbarked)
        {
            return false;
        }

        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            BaseTraitEffect traitInfo = activeData.activeTraits[i].GetTraitInfo();
            if (!traitInfo.CanEmbark())
            {
                return false;
            }
        }

        return true;
    }

    public void EmbarkTraits()
    {
        activeData.EmbarkTraits();
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].activeData.EmbarkTraits();
        }
    }

    public void DisembarkTraits()
    {
        bool hasDeter = IsCardTraitsDetered();
        activeData.DisembarkTraits(hasDeter);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].activeData.DisembarkTraits(hasDeter);
        }
    }

    public override bool CanDeploy(CardStack target, RegionEnum region, bool emptyAvailable, bool embark)
    {
        CardTemplate cardTemplate = GetTemplate();
        bool flag = cardTemplate.CanDeploy(region, activeData.owner);
        if (!flag)
        {
            flag = CanOverrideDeploy(region);
        }

        if (!cardTemplate.CanDeploy(target, emptyAvailable, embark))
        {
            Console.WriteLine("UnitCard.CanDeploy false - Template check failed");
            return false;
        }

        if (embark && target.primaryCard != null)
        {
            Card primaryCard = target.primaryCard;
            if (primaryCard.activeData.owner != activeData.owner)
            {
                Console.WriteLine("UnitCard.CanDeploy false - invalid ebark owner");
                return false;
            }

            if (!CanEmbark() || !primaryCard.CanEmbark())
            {
                Console.WriteLine("UnitCard.CanDeploy false - ineligable ebark unit");
                return false;
            }

            for (int i = 0; i < cardTraits.Length; i++)
            {
                BaseTrait baseTrait = cardTraits[i];
                if (baseTrait == null)
                {
                    continue;
                }

                for (int j = 0; j < cardTraits[i].effects.Count; j++)
                {
                    BaseTraitEffect baseTraitEffect = cardTraits[i].effects[j];
                    if (!baseTraitEffect.CanEmbark())
                    {
                        Console.WriteLine("UnitCard.CanDeploy false - trait prevents embark " +
                                          baseTraitEffect.effectTraitID);
                        return false;
                    }
                }
            }
        }
        else if (!flag)
        {
            Console.WriteLine("UnitCard.CanDeploy false - Template check failed");
            return false;
        }

        for (int k = 0; k < cardTraits.Length; k++)
        {
            BaseTrait baseTrait2 = cardTraits[k];
            if (baseTrait2 == null)
            {
                continue;
            }

            for (int l = 0; l < cardTraits[k].effects.Count; l++)
            {
                BaseTraitEffect baseTraitEffect2 = cardTraits[k].effects[l];
                if (!baseTraitEffect2.CanDeploy(target, region))
                {
                    Console.WriteLine("UnitCard.CanDeploy false - trait prevents deploy " +
                                      baseTraitEffect2.effectTraitID);
                    return false;
                }
            }
        }

        return true;
    }

    public override bool Deploy(CardStack stack, bool embark, RegionEnum target, CardTransitionCCGEvent deployEvent)
    {
        if (base.Deploy(stack, embark, target, deployEvent))
        {
            CheckAndUpdateXP("Deploy");
            return true;
        }

        if (embark)
        {
            Card primaryCard = stack.primaryCard;
            CardType type = GetTemplate().Type;
            BaseTrait baseTrait = null;
            bool flag = primaryCard.HasAnyActionsAvailable();
            if (type == CardType.Titan && primaryCard.GetTemplate().Type == CardType.Pilot)
            {
                SetActed(15);
                stack.primaryCard = this;
                UnitCard unitCard = (UnitCard) primaryCard;
                unitCard.pilotEmbarked = true;
                embarkedPilot = unitCard;
                if (deployEvent != null)
                {
                    deployEvent.embark = true;
                    deployEvent.targetId = unitCard.instanceId;
                    deployEvent.targetOwner = unitCard.activeData.owner;
                }

                for (int i = 0; i < cardTraits.Length; i++)
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

                CheckAndUpdateXP("Deploy");
                return true;
            }

            if (type == CardType.Pilot && primaryCard.GetTemplate().Type == CardType.Titan)
            {
                SetActed(15);
                UnitCard unitCard2 = (UnitCard) primaryCard;
                unitCard2.embarkedPilot = this;
                pilotEmbarked = true;
                if (deployEvent != null)
                {
                    deployEvent.embark = true;
                    deployEvent.targetId = unitCard2.instanceId;
                    deployEvent.targetOwner = unitCard2.activeData.owner;
                }

                for (int j = 0; j < cardTraits.Length; j++)
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

                CheckAndUpdateXP("Deploy");
                return true;
            }

            Console.WriteLine("DEPLOY FAILED - UnitCard.Deploy - invalid embark combo");
        }

        return false;
    }

    public override bool Move(CardStack target, RegionEnum region, RegionEnum origin, bool embark)
    {
        if (base.Move(target, region, origin, embark))
        {
            return true;
        }

        if (embark)
        {
            CardType type = GetTemplate().Type;
            UnitCard unitCard = (UnitCard) target.primaryCard;
            CardType type2 = unitCard.GetTemplate().Type;
            bool flag = unitCard.HasAnyActionsAvailable();
            if (type == CardType.Titan && type2 == CardType.Pilot)
            {
                SetActed(14);
                target.primaryCard = this;
                embarkedPilot = unitCard;
                unitCard.pilotEmbarked = true;
                EmbarkTraits();
                embarkedPilot.EmbarkTraits();
                GameState.GetPilotEmbarkTrait().Activate(embarkedPilot, target, region, GameState);
                if (flag)
                {
                    GameState.GetTitanPilotEmbarkTrait().Activate(this, target, region, GameState);
                }

                return true;
            }

            if (type == CardType.Pilot && type2 == CardType.Titan)
            {
                SetActed(14);
                pilotEmbarked = true;
                unitCard.embarkedPilot = this;
                EmbarkTraits();
                unitCard.EmbarkTraits();
                GameState.GetPilotEmbarkTrait().Activate(this, target, region, GameState);
                if (flag)
                {
                    GameState.GetTitanPilotEmbarkTrait().Activate(unitCard, target, region, GameState);
                }

                return true;
            }

            Console.WriteLine("MOVE FAILED - UnitCard.Move - invalid embark combo");
        }

        return false;
    }

    public override bool Disembark(CardStack location, RegionEnum region)
    {
        SetActed(14);
        embarkedPilot = null;
        pilotEmbarked = false;
        activeData.MoveTraits(location, region, false);
        return true;
    }

    public override bool HasAttack()
    {
        return attack > 0;
    }

    public override bool CanAttack(CardStack source, CardStack target)
    {
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            ActiveTrait activeTrait = activeData.activeTraits[i];
            if (!activeTrait.GetTraitInfo().CanAttack(target, activeTrait))
            {
                Console.WriteLine("UnitCard.CanAttack false - trait prevetns attack " + activeTrait.traitEffectId);
                return false;
            }
        }

        if (base.CanAttack(source, target) && target.primaryCard != null)
        {
            if (activeData.owner != target.primaryCard.activeData.owner)
            {
                if (GetTemplate().CanAttack(source, target))
                {
                    if (attack > 0)
                    {
                        return true;
                    }

                    Console.WriteLine("UnitCard.CanAttack false - card base attack is 0");
                }
                else
                {
                    Console.WriteLine("UnitCard.CanAttack false - template check failed");
                }
            }
            else
            {
                Console.WriteLine("UnitCard.CanAttack false - target owner is same as attacker");
            }
        }

        return false;
    }

    public override void Attack(CardStack source, Card target)
    {
        int targetID = ((target != null) ? target.instanceId : (-1));
        sbyte targetOwner = (sbyte) ((target != null) ? target.activeData.owner : (-1));
        CombatCCGEvent combatCCGEvent = null;
        CombatCCGEvent combatCCGEvent2 = new CombatCCGEvent(CCGEventType.CombatStart, instanceId, activeData.owner,
            targetID, targetOwner, 0, 0);
        GameState.AddCCGEventLog(combatCCGEvent2);
        if (target != null)
        {
            RegionEnum traitActorRegion = GameState.GetTraitActorRegion(target.activeData.owner, target.instanceId);
            ActiveTrait activeTrait = null;
            List<CardStack> list = GameState.FindCardStack(target);
            CardStack cardStack = null;
            if (list.Count > 0)
            {
                cardStack = list[0];
            }

            for (int i = 0; i < cardTraits.Length; i++)
            {
                if (cardTraits[i].traitType == TraitType.Assault && !activeData.traitActivated[i])
                {
                    activeData.traitActivated[i] = true;
                    cardTraits[i].Activate(this, cardStack, traitActorRegion, GameState);
                }
            }

            sbyte b = 0;
            sbyte bypass = 0;
            if (CanDiscard() || target.CanDiscard())
            {
                SetActed(14);
                activeData.AttackTraits(target);
                combatCCGEvent = new CombatCCGEvent(CCGEventType.CombatEnd, instanceId, activeData.owner, targetID,
                    targetOwner, 0, 0);
                GameState.AddCCGEventLog(combatCCGEvent);
                return;
            }

            CombatCCGEvent combatCCGEvent3 = new CombatCCGEvent(CCGEventType.CombatAttack, instanceId, activeData.owner,
                target.instanceId, target.activeData.owner, b, bypass);
            combatCCGEvent3.result = 0;
            GameState.AddCCGEventLog(combatCCGEvent3);
            GameState.CardAttacked(this, target);
            b = GetCurrentAttack(target, true);
            bypass = GetCurrentBypassDefense(target, true);
            target.GetCurrentDefense(true);
            target.GetCurrentHealth(true);
            List<EventLogTraitCardInfo> list2 = new List<EventLogTraitCardInfo>();
            for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
            {
                activeTrait = activeData.activeTraits[num];
                if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(8, activeTrait))
                {
                    bypass += b;
                    b = 0;
                    EventLogTraitCardInfo eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.instanceId = activeTrait.GetTraitSource().instanceId;
                    eventLogTraitCardInfo.owner = activeTrait.GetTraitSource().activeData.owner;
                    eventLogTraitCardInfo.effectID = activeTrait.GetTraitInfo().effectTraitID;
                    eventLogTraitCardInfo.traitID = activeTrait.GetTraitInfo().traitParentID;
                    eventLogTraitCardInfo.data = 1;
                    list2.Add(eventLogTraitCardInfo);
                    if (activeTrait.HasCharges())
                    {
                        activeTrait.ExpendCharge(GameState);
                    }
                }
                else if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(9, activeTrait))
                {
                    b += bypass;
                    bypass = 0;
                    EventLogTraitCardInfo eventLogTraitCardInfo2 = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo2.instanceId = activeTrait.GetTraitSource().instanceId;
                    eventLogTraitCardInfo2.owner = activeTrait.GetTraitSource().activeData.owner;
                    eventLogTraitCardInfo2.effectID = activeTrait.GetTraitInfo().effectTraitID;
                    eventLogTraitCardInfo2.traitID = activeTrait.GetTraitInfo().traitParentID;
                    eventLogTraitCardInfo2.data = 0;
                    list2.Add(eventLogTraitCardInfo2);
                    if (activeTrait.HasCharges())
                    {
                        activeTrait.ExpendCharge(GameState);
                    }
                }
            }

            if (list2.Count > 0)
            {
                CombatBuffsCCGEvent combatBuffsCCGEvent = new CombatBuffsCCGEvent(CCGEventType.CombatBuffsConversion,
                    instanceId, activeData.owner, targetID, targetOwner);
                combatBuffsCCGEvent.buffTraits = new EventLogTraitCardInfo[list2.Count];
                for (int j = 0; j < list2.Count; j++)
                {
                    combatBuffsCCGEvent.buffTraits[j] = list2[j];
                }

                GameState.AddCCGEventLog(combatBuffsCCGEvent);
            }

            combatCCGEvent3.attackTotal = b;
            combatCCGEvent3.bypassTotal = bypass;
            if (!CanAttack(source, cardStack) || CanDiscard() || target.CanDiscard())
            {
                SetActed(14);
                activeData.AttackTraits(target);
                combatCCGEvent = new CombatCCGEvent(CCGEventType.CombatEnd, instanceId, activeData.owner, targetID,
                    targetOwner, 0, 0);
                GameState.AddCCGEventLog(combatCCGEvent);
                return;
            }

            if (target.CanCounterAttack(cardStack, source, false))
            {
                combatCCGEvent2.result = 1;
            }

            if ((combatCCGEvent3.result = ValidateAttackEffect(this, target)) == 0)
            {
                target.TakeDamage(b, bypass, this, false);
            }
        }

        base.Attack(source, target);
        combatCCGEvent = new CombatCCGEvent(CCGEventType.CombatEnd, instanceId, activeData.owner, targetID, targetOwner,
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
            for (int i = 0; i < activeData.activeTraits.Count; i++)
            {
                activeTrait = activeData.activeTraits[i];
                if (!activeTrait.GetTraitInfo().CanCounterAttack(target, activeTrait))
                {
                    if (inCombat && activeTrait.HasCharges())
                    {
                        activeTrait.ExpendCharge(GameState);
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
        CombatCCGEvent combatCCGEvent = new CombatCCGEvent(CCGEventType.CombatCounter, instanceId, activeData.owner,
            target.instanceId, target.activeData.owner, b, bypass);
        combatCCGEvent.result = 0;
        GameState.AddCCGEventLog(combatCCGEvent);
        GameState.CardCounterAttacked(this, target);
        b = GetCurrentAttack(target, true);
        bypass = GetCurrentBypassDefense(target, true);
        target.GetCurrentDefense(true);
        target.GetCurrentHealth(true);
        List<EventLogTraitCardInfo> list = new List<EventLogTraitCardInfo>();
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = activeData.activeTraits[num];
            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(8, activeTrait))
            {
                bypass += b;
                b = 0;
                EventLogTraitCardInfo eventLogTraitCardInfo = new EventLogTraitCardInfo();
                eventLogTraitCardInfo.instanceId = activeTrait.GetTraitSource().instanceId;
                eventLogTraitCardInfo.owner = activeTrait.GetTraitSource().activeData.owner;
                eventLogTraitCardInfo.effectID = activeTrait.GetTraitInfo().effectTraitID;
                eventLogTraitCardInfo.traitID = activeTrait.GetTraitInfo().traitParentID;
                eventLogTraitCardInfo.data = 1;
                list.Add(eventLogTraitCardInfo);
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }
            }
            else if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(9, activeTrait))
            {
                b += bypass;
                bypass = 0;
                EventLogTraitCardInfo eventLogTraitCardInfo2 = new EventLogTraitCardInfo();
                eventLogTraitCardInfo2.instanceId = activeTrait.GetTraitSource().instanceId;
                eventLogTraitCardInfo2.owner = activeTrait.GetTraitSource().activeData.owner;
                eventLogTraitCardInfo2.effectID = activeTrait.GetTraitInfo().effectTraitID;
                eventLogTraitCardInfo2.traitID = activeTrait.GetTraitInfo().traitParentID;
                eventLogTraitCardInfo2.data = 0;
                list.Add(eventLogTraitCardInfo2);
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }
            }
        }

        if (list.Count > 0)
        {
            int targetID = ((target != null) ? target.instanceId : (-1));
            sbyte targetOwner = (sbyte) ((target != null) ? target.activeData.owner : (-1));
            CombatBuffsCCGEvent combatBuffsCCGEvent = new CombatBuffsCCGEvent(CCGEventType.CombatBuffsConversion,
                instanceId, activeData.owner, targetID, targetOwner);
            combatBuffsCCGEvent.buffTraits = new EventLogTraitCardInfo[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                combatBuffsCCGEvent.buffTraits[i] = list[i];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        sbyte b2 = (combatCCGEvent.result = ValidateCounterAttackEffect(this, target));
        combatCCGEvent.attackTotal = b;
        combatCCGEvent.bypassTotal = bypass;
        if (b2 == 0)
        {
            target.TakeDamage(b, bypass, this, false);
        }

        base.CounterAttack(source, target);
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        sbyte b = attack;
        sbyte bypass2 = bypass;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            if (activeTrait.GetTraitInfo().IsDamageImmunity(false, activeTrait))
            {
                b = 0;
            }

            if (activeTrait.GetTraitInfo().IsDamageImmunity(true, activeTrait))
            {
                bypass2 = 0;
            }
        }

        sbyte b2 = GetCurrentDefense(false);
        if (b2 < 0)
        {
            b2 = 0;
        }

        sbyte b3 = ((b < b2) ? b : b2);
        TakeDefenseDamage(b3);
        base.TakeDamage((sbyte) (b - b3), bypass2, source, checkDeath);
    }

    public override void CreateActiveData()
    {
        if (activeData == null)
        {
            activeData = new ActiveUnitCardData();
            activeData.Setup(this);
        }
    }

    public override void InitActiveData()
    {
        if (activeData != null)
        {
            base.InitActiveData();
        }

        UnitCardTemplate unitTemplate = (UnitCardTemplate) GetTemplate();
        bypassDefense = 0;
        attack = unitTemplate.Attack;
        defense = unitTemplate.Defense;
        if (embarkedPilot != null)
        {
            embarkedPilot.InitActiveData();
        }
    }

    private sbyte ValidateAttackEffect(Card source, Card target)
    {
        ActiveTrait activeTrait = null;
        bool flag = false;
        for (int num = source.activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = source.activeData.activeTraits[num];
            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(2, activeTrait))
            {
                flag = true;
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                break;
            }
        }

        for (int num2 = target.activeData.activeTraits.Count - 1; num2 >= 0; num2--)
        {
            activeTrait = target.activeData.activeTraits[num2];
            if (!flag && activeTrait.GetTraitInfo().IsCombatManipulationPassive(1, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                return 1;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(3, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                return 3;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(6, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                return 6;
            }
        }

        return 0;
    }

    public override bool IsImmobile()
    {
        if (HasStatusEffect(1) || HasStatusEffect(4))
        {
            return true;
        }

        return false;
    }

    public bool IsEmbarked()
    {
        return pilotEmbarked || embarkedPilot != null;
    }

    public override bool HasPilot()
    {
        if (template.Type != CardType.Titan)
        {
            return false;
        }

        return embarkedPilot != null;
    }

    public override EntityCard GetEmbarkedPilot()
    {
        return embarkedPilot;
    }

    public override void CardDeployed(Card deployed)
    {
        base.CardDeployed(deployed);
        if (HasPilot())
        {
            embarkedPilot.CardDeployed(deployed);
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        if (HasPilot())
        {
            embarkedPilot.NewTurn(playerIndex);
        }
    }

    public override void EndTurn(sbyte playerIndex)
    {
        base.EndTurn(playerIndex);
        ResetDefense();
        if (HasPilot())
        {
            embarkedPilot.EndTurn(playerIndex);
        }
    }

    public override void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        base.CardMoved(card, target, region, origin);
        if (HasPilot())
        {
            embarkedPilot.CardMoved(card, target, region, origin);
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        if (HasPilot())
        {
            embarkedPilot.CardAttacked(attacker, target);
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        if (HasPilot())
        {
            embarkedPilot.CardCounterAttacked(attacker, target);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        if (HasPilot())
        {
            embarkedPilot.CardGainedStatus(theCard, source, statusType);
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        if (HasPilot())
        {
            embarkedPilot.CardDamaged(damagedCard, source);
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        if (HasPilot())
        {
            embarkedPilot.CardDied(deadCard, source);
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        if (HasPilot())
        {
            embarkedPilot.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        if (HasPilot())
        {
            embarkedPilot.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public override void SecretTriggered(Card secret, Card source)
    {
        base.SecretTriggered(secret, source);
        if (HasPilot())
        {
            embarkedPilot.SecretTriggered(secret, source);
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        if (HasPilot())
        {
            embarkedPilot.SecretDestroyed(secret, source);
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        if (HasPilot())
        {
            embarkedPilot.TraitEffectActivating(effect, source, target, region);
        }
    }

    public void CheckAndUpdateXP(string xpTrigger)
    {
        int trigger = RulesetParser.GetXpTrigger(xpTrigger);
        if (trigger > 0)
        {
            xp += trigger;
            CardInfoCCGEvent logData = new CardInfoCCGEvent(CCGEventType.CardXPEarned, instanceId, activeData.owner,
                trigger, xpTrigger);
            GameState.AddCCGEventLog(logData);
        }
    }

    public override void Discard(Player[] players)
    {
        base.Discard(players);
        if (HasPilot())
        {
            embarkedPilot.Discard(players);
        }
    }

    private sbyte ValidateCounterAttackEffect(Card source, Card target)
    {
        ActiveTrait activeTrait = null;
        bool flag = false;
        for (int num = source.activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = source.activeData.activeTraits[num];
            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(5, activeTrait))
            {
                flag = true;
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                break;
            }
        }

        for (int i = 0; i < target.activeData.activeTraits.Count; i++)
        {
            activeTrait = target.activeData.activeTraits[i];
            if (!flag && activeTrait.GetTraitInfo().IsCombatManipulationPassive(4, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                return 4;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(6, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                return 6;
            }

            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(3, activeTrait))
            {
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }

                return 3;
            }
        }

        return 0;
    }

    private void ResetDefense()
    {
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) activeData;
        activeUnitCardData.currentDefense = GetMaxDefense();
    }
}