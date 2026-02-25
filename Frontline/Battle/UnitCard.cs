using Frontline.Battle.CcgEvents;
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
            EmbarkedPilot = unitCard.EmbarkedPilot;
            PilotEmbarked = unitCard.PilotEmbarked;
        }
    }

    public UnitCard(CCG game, ItemEntity itemEntity)
        : base (game, itemEntity)
    {
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
        if (EmbarkedPilot != null)
        {
            EmbarkedPilot = (UnitCard) EmbarkedPilot.GenerateAndInit(GameState);
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

        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
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
            int count = list.Count;
            int targetID = ((target != null) ? target.InstanceId : (-1));
            sbyte targetOwner = (sbyte) ((target != null) ? target.ActiveData.Owner : (-1));
            CombatBuffsCcgEvent combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack,
                InstanceId, ActiveData.Owner, targetID, targetOwner);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
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

        sbyte b = bypassDefense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
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
            int count = list.Count;
            int targetID = ((target != null) ? target.InstanceId : (-1));
            sbyte targetOwner = (sbyte) ((target != null) ? target.ActiveData.Owner : (-1));
            CombatBuffsCcgEvent combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack,
                InstanceId, ActiveData.Owner, targetID, targetOwner);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public override sbyte GetCurrentDefense(bool combatLog)
    {
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) ActiveData;
        sbyte b = activeUnitCardData.CurrentDefense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
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
            int count = list.Count;
            CombatBuffsCcgEvent combatBuffsCCGEvent =
                new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack, InstanceId, ActiveData.Owner, 0, 0);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public void SetCurrentDefense(sbyte newDefense)
    {
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) ActiveData;
        activeUnitCardData.CurrentDefense = newDefense;
    }

    public void TakeDefenseDamage(sbyte damage)
    {
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) ActiveData;
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

        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            BaseTraitEffect traitInfo = ActiveData.ActiveTraits[i].GetTraitInfo();
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
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].ActiveData.EmbarkTraits();
        }
    }

    public void DisembarkTraits()
    {
        bool hasDeter = IsCardTraitsDetered();
        ActiveData.DisembarkTraits(hasDeter);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].ActiveData.DisembarkTraits(hasDeter);
        }
    }

    public override bool CanDeploy(CardStack target, Region region, bool emptyAvailable, bool embark)
    {
        CardTemplate cardTemplate = GetTemplate();
        bool flag = cardTemplate.CanDeploy(region, ActiveData.Owner);
        if (!flag)
        {
            flag = CanOverrideDeploy(region);
        }

        if (!cardTemplate.CanDeploy(target, emptyAvailable, embark))
        {
            Console.WriteLine("UnitCard.CanDeploy false - Template check failed");
            return false;
        }

        if (embark && target.PrimaryCard != null)
        {
            Card primaryCard = target.PrimaryCard;
            if (primaryCard.ActiveData.Owner != ActiveData.Owner)
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

                for (int j = 0; j < cardTraits[i].Effects.Count; j++)
                {
                    BaseTraitEffect baseTraitEffect = cardTraits[i].Effects[j];
                    if (!baseTraitEffect.CanEmbark())
                    {
                        Console.WriteLine("UnitCard.CanDeploy false - trait prevents embark " +
                                          baseTraitEffect.EffectTraitId);
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

            for (int l = 0; l < cardTraits[k].Effects.Count; l++)
            {
                BaseTraitEffect baseTraitEffect2 = cardTraits[k].Effects[l];
                if (!baseTraitEffect2.CanDeploy(target, region))
                {
                    Console.WriteLine("UnitCard.CanDeploy false - trait prevents deploy " +
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
            CheckAndUpdateXP("Deploy");
            return true;
        }

        if (embark)
        {
            Card primaryCard = stack.PrimaryCard;
            CardType type = GetTemplate().Type;
            BaseTrait baseTrait = null;
            bool flag = primaryCard.HasAnyActionsAvailable();
            if (type == CardType.Titan && primaryCard.GetTemplate().Type == CardType.Pilot)
            {
                SetActed(15);
                stack.PrimaryCard = this;
                UnitCard unitCard = (UnitCard) primaryCard;
                unitCard.PilotEmbarked = true;
                EmbarkedPilot = unitCard;
                if (deployEvent != null)
                {
                    deployEvent.Embark = true;
                    deployEvent.TargetId = unitCard.InstanceId;
                    deployEvent.TargetOwner = unitCard.ActiveData.Owner;
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
                unitCard2.EmbarkedPilot = this;
                PilotEmbarked = true;
                if (deployEvent != null)
                {
                    deployEvent.Embark = true;
                    deployEvent.TargetId = unitCard2.InstanceId;
                    deployEvent.TargetOwner = unitCard2.ActiveData.Owner;
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

    public override bool Move(CardStack target, Region region, Region origin, bool embark)
    {
        if (base.Move(target, region, origin, embark))
        {
            return true;
        }

        if (embark)
        {
            CardType type = GetTemplate().Type;
            UnitCard unitCard = (UnitCard) target.PrimaryCard;
            CardType type2 = unitCard.GetTemplate().Type;
            bool flag = unitCard.HasAnyActionsAvailable();
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

            Console.WriteLine("MOVE FAILED - UnitCard.Move - invalid embark combo");
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
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            ActiveTrait activeTrait = ActiveData.ActiveTraits[i];
            if (!activeTrait.GetTraitInfo().CanAttack(target, activeTrait))
            {
                Console.WriteLine("UnitCard.CanAttack false - trait prevetns attack " + activeTrait.TraitEffectId);
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
        int targetID = ((target != null) ? target.InstanceId : (-1));
        sbyte targetOwner = (sbyte) ((target != null) ? target.ActiveData.Owner : (-1));
        CombatCcgEvent combatCCGEvent = null;
        CombatCcgEvent combatCCGEvent2 = new CombatCcgEvent(CcgEventType.CombatStart, InstanceId, ActiveData.Owner,
            targetID, targetOwner, 0, 0);
        GameState.AddCCGEventLog(combatCCGEvent2);
        if (target != null)
        {
            Region traitActorRegion = GameState.GetTraitActorRegion(target.ActiveData.Owner, target.InstanceId);
            ActiveTrait activeTrait = null;
            List<CardStack> list = GameState.FindCardStack(target);
            CardStack cardStack = null;
            if (list.Count > 0)
            {
                cardStack = list[0];
            }

            for (int i = 0; i < cardTraits.Length; i++)
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

            CombatCcgEvent combatCCGEvent3 = new CombatCcgEvent(CcgEventType.CombatAttack, InstanceId, ActiveData.Owner,
                target.InstanceId, target.ActiveData.Owner, b, bypass);
            combatCCGEvent3.Result = 0;
            GameState.AddCCGEventLog(combatCCGEvent3);
            GameState.CardAttacked(this, target);
            b = GetCurrentAttack(target, true);
            bypass = GetCurrentBypassDefense(target, true);
            target.GetCurrentDefense(true);
            target.GetCurrentHealth(true);
            List<EventLogTraitCardInfo> list2 = new List<EventLogTraitCardInfo>();
            for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
            {
                activeTrait = ActiveData.ActiveTraits[num];
                if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(8, activeTrait))
                {
                    bypass += b;
                    b = 0;
                    EventLogTraitCardInfo eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo.Data = 1;
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
                    eventLogTraitCardInfo2.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo2.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo2.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo2.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo2.Data = 0;
                    list2.Add(eventLogTraitCardInfo2);
                    if (activeTrait.HasCharges())
                    {
                        activeTrait.ExpendCharge(GameState);
                    }
                }
            }

            if (list2.Count > 0)
            {
                CombatBuffsCcgEvent combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsConversion,
                    InstanceId, ActiveData.Owner, targetID, targetOwner);
                combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[list2.Count];
                for (int j = 0; j < list2.Count; j++)
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
            for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
            {
                activeTrait = ActiveData.ActiveTraits[i];
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
        CombatCcgEvent combatCCGEvent = new CombatCcgEvent(CcgEventType.CombatCounter, InstanceId, ActiveData.Owner,
            target.InstanceId, target.ActiveData.Owner, b, bypass);
        combatCCGEvent.Result = 0;
        GameState.AddCCGEventLog(combatCCGEvent);
        GameState.CardCounterAttacked(this, target);
        b = GetCurrentAttack(target, true);
        bypass = GetCurrentBypassDefense(target, true);
        target.GetCurrentDefense(true);
        target.GetCurrentHealth(true);
        List<EventLogTraitCardInfo> list = new List<EventLogTraitCardInfo>();
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = ActiveData.ActiveTraits[num];
            if (activeTrait.GetTraitInfo().IsCombatManipulationPassive(8, activeTrait))
            {
                bypass += b;
                b = 0;
                EventLogTraitCardInfo eventLogTraitCardInfo = new EventLogTraitCardInfo();
                eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                eventLogTraitCardInfo.Data = 1;
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
                eventLogTraitCardInfo2.InstanceId = activeTrait.GetTraitSource().InstanceId;
                eventLogTraitCardInfo2.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                eventLogTraitCardInfo2.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                eventLogTraitCardInfo2.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                eventLogTraitCardInfo2.Data = 0;
                list.Add(eventLogTraitCardInfo2);
                if (activeTrait.HasCharges())
                {
                    activeTrait.ExpendCharge(GameState);
                }
            }
        }

        if (list.Count > 0)
        {
            int targetID = ((target != null) ? target.InstanceId : (-1));
            sbyte targetOwner = (sbyte) ((target != null) ? target.ActiveData.Owner : (-1));
            CombatBuffsCcgEvent combatBuffsCCGEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsConversion,
                InstanceId, ActiveData.Owner, targetID, targetOwner);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                combatBuffsCCGEvent.BuffTraits[i] = list[i];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        sbyte b2 = (combatCCGEvent.Result = ValidateCounterAttackEffect(this, target));
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
        sbyte b = attack;
        sbyte bypass2 = bypass;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
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

        UnitCardTemplate unitTemplate = (UnitCardTemplate) GetTemplate();
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
        bool flag = false;
        for (int num = source.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = source.ActiveData.ActiveTraits[num];
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

        for (int num2 = target.ActiveData.ActiveTraits.Count - 1; num2 >= 0; num2--)
        {
            activeTrait = target.ActiveData.ActiveTraits[num2];
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

    public void CheckAndUpdateXP(string xpTrigger)
    {
        int trigger = RulesetParser.GetXpTrigger(xpTrigger);
        if (trigger > 0)
        {
            Xp += trigger;
            CardInfoCcgEvent logData = new CardInfoCcgEvent(CcgEventType.CardXpEarned, InstanceId, ActiveData.Owner,
                trigger, xpTrigger);
            GameState.AddCCGEventLog(logData);
        }
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
        bool flag = false;
        for (int num = source.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            activeTrait = source.ActiveData.ActiveTraits[num];
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

        for (int i = 0; i < target.ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = target.ActiveData.ActiveTraits[i];
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
        ActiveUnitCardData activeUnitCardData = (ActiveUnitCardData) ActiveData;
        activeUnitCardData.CurrentDefense = GetMaxDefense();
    }
}