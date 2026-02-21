using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class EntityCard : Card
{
    public List<Card> secrets;

    private sbyte maxHealth;

    private Card myDeathCard;

    private bool isDead;

    public EntityCard(CCG game)
        : base(game)
    {
    }

    public EntityCard(CCG game, Card other)
        : base(game, other)
    {
        secrets = other.GetSecrets();
    }

    public override void Setup()
    {
        base.Setup();
        EntityCardTemplate entityTemplate = (EntityCardTemplate) GetTemplate();
        maxHealth = entityTemplate.Health;
        secrets = new List<Card>();
        myDeathCard = null;
        isDead = false;
    }

    public override List<Card> GetSecrets()
    {
        return secrets;
    }

    public override void InitStackedCards()
    {
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num] = secrets[num].GenerateAndInit(GameState);
            }
        }

        base.InitStackedCards();
    }

    public override bool CanDeployAnywhere()
    {
        return false;
    }

    public override Card FindTraitActor(int cardId, sbyte ownerId)
    {
        Card card = base.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        if (secrets == null)
        {
            return card;
        }

        for (int i = 0; i < secrets.Count; i++)
        {
            card = secrets[i];
            if (card.instanceId == cardId && card.activeData.owner == ownerId)
            {
                return card;
            }
        }

        return null;
    }

    public override bool DoesMatchTargetingInfo(TraitTargeting info, Card source)
    {
        if (base.DoesMatchTargetingInfo(info, source))
        {
            return true;
        }

        if (secrets == null)
        {
            return true;
        }

        for (int i = 0; i < secrets.Count; i++)
        {
            if (secrets[i].DoesMatchTargetingInfo(info, source))
            {
                return true;
            }
        }

        return false;
    }

    public override UnitType GetUnitType()
    {
        return UnitType.None;
    }

    public override bool Deploy(CardStack stack, bool embark, RegionEnum target, CardTransitionCCGEvent deployEvent)
    {
        if (stack.primaryCard == null)
        {
            SetActed(15);
            stack.primaryCard = this;
            for (int i = 0; i < cardTraits.Length; i++)
            {
                BaseTrait baseTrait = cardTraits[i];
                if (baseTrait != null && baseTrait.ActivateOnDeploy())
                {
                    baseTrait.Activate(this, stack, target, GameState);
                }
            }

            return true;
        }

        if (!embark)
        {
            Console.WriteLine("DEPLOY FAILED - EntityCard.Deploy target cardstack was not empty ID" +
                              stack.primaryCard.instanceId);
        }

        return false;
    }

    public override bool Move(CardStack target, RegionEnum region, RegionEnum origin, bool embark)
    {
        if (target.primaryCard != null)
        {
            if (!embark)
            {
                Console.WriteLine("MOVE FAILED - EntityCard.Move - target CardStack not empty. CID-" +
                                  target.primaryCard.instanceId);
            }

            return false;
        }

        target.primaryCard = this;
        SetActed(14);
        activeData.MoveTraits(target, region, embark);
        return base.Move(target, region, origin, embark);
    }

    public override bool HasActed()
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        return activeEntityCardData.acted != 0;
    }

    public override bool HasActed(sbyte actions)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        return ((byte) activeEntityCardData.acted & (byte) actions) != 0;
    }

    public override bool HasAnyActionsAvailable()
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        return (((byte) activeEntityCardData.acted & 0xE) ^ 0xE) != 0;
    }

    public override bool CanActivate(Card target, RegionEnum region)
    {
        if (HasActed(8))
        {
            return false;
        }

        BaseTrait activationTrait = GetActivationTrait();
        if (activationTrait == null)
        {
            return false;
        }

        BaseTraitEffect primaryTargeting = activationTrait.GetPrimaryTargeting(0);
        if (primaryTargeting.targets.HasAreaTarget())
        {
            return activationTrait.CanActivate(region, activeData.owner);
        }

        if (target == null)
        {
            return false;
        }

        return activationTrait.CanActivate(target, this, region, GameState);
    }

    public override bool CanAttack(CardStack source, CardStack target)
    {
        if (target.primaryCard == null)
        {
            Console.WriteLine("EntityCard.CanAttack false - Target stack is empty");
            return false;
        }

        if (CanDiscard())
        {
            Console.WriteLine("EntityCard.CanAttack false - card is dead");
            return false;
        }

        if (IgnoresIntercept())
        {
            return true;
        }

        bool flag = false;
        bool flag2 = GameState.HasInterceptBattleEffect(activeData.owner);
        if (flag2)
        {
            flag = target.primaryCard.HasIntercept();
        }

        if (flag2 && !flag)
        {
            Console.WriteLine("EntityCard.CanAttack false - target card is not intercept");
        }

        return !flag2 || flag;
    }

    public override void Attack(CardStack source, Card target)
    {
        if (target == null)
        {
            return;
        }

        SetActed(14);
        activeData.AttackTraits(target);
        List<CardStack> list = GameState.FindCardStack(target);
        if (list.Count > 0)
        {
            CardStack source2 = list[0];
            if (target.CanCounterAttack(source2, source, true))
            {
                target.CounterAttack(source2, this);
            }
        }
    }

    public void ActivateTrait(CardStack target, RegionEnum region, CCG game)
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].traitType == TraitType.OneShot && !activeData.traitActivated[i])
            {
                SetActed(14);
                activeData.traitActivated[i] = true;
                for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
                {
                    activeTrait = activeData.activeTraits[num];
                    activeTrait.GetTraitInfo().ActivateAction(target, region, activeTrait);
                }

                cardTraits[i].Activate(this, target, region, game);
                break;
            }
        }
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        sbyte currentHealth = activeEntityCardData.currentHealth;
        sbyte b = attack;
        sbyte b2 = bypass;
        sbyte b3 = 0;
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
                b2 = 0;
            }
        }

        b3 = (sbyte) (b + b2);
        if (b3 > 0)
        {
            SetCurrentHealth((sbyte) (currentHealth - b3));
            GameState.CardDamaged(this, source);
            CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CCGEventType.CardDamage, b3, source.instanceId,
                source.activeData.owner, instanceId, activeData.owner);
            GameState.AddCCGEventLog(logData);
            if (myDeathCard == null && CanDiscard())
            {
                myDeathCard = source;
            }

            if (checkDeath)
            {
                CheckForDeathEvent();
            }
        }
    }

    public override void CheckForDeathEvent()
    {
        if (myDeathCard == null || isDead)
        {
            return;
        }

        isDead = true;
        RegionEnum traitActorRegion = GameState.GetTraitActorRegion(activeData.owner, instanceId);
        List<CardStack> list = GameState.FindCardStack(this);
        CardStack cardStack = null;
        if (list == null || list.Count == 0)
        {
            return;
        }

        cardStack = list[0];
        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].traitType == TraitType.LastStand && !activeData.traitActivated[i])
            {
                activeData.traitActivated[i] = true;
                cardTraits[i].Activate(this, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(this, myDeathCard);
        CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CCGEventType.CardDeath, GetCurrentHealth(false),
            myDeathCard.instanceId, myDeathCard.activeData.owner, instanceId, activeData.owner);
        GameState.AddCCGEventLog(logData);
        if (myDeathCard.GetTemplate().IsCombatUnit())
        {
            UnitCard unitCard = (UnitCard) myDeathCard;
            string xpTrigger = "Destroy_" + template.Type;
            unitCard.CheckAndUpdateXP(xpTrigger);
            if (unitCard.HasPilot())
            {
                unitCard.embarkedPilot.CheckAndUpdateXP(xpTrigger);
            }
        }

        if (!HasPilot())
        {
            return;
        }

        EntityCard embarkedPilot = GetEmbarkedPilot();
        for (int j = 0; j < embarkedPilot.cardTraits.Length; j++)
        {
            if (embarkedPilot.cardTraits[j].traitType == TraitType.LastStand &&
                !embarkedPilot.activeData.traitActivated[j])
            {
                embarkedPilot.activeData.traitActivated[j] = true;
                embarkedPilot.cardTraits[j].Activate(embarkedPilot, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(embarkedPilot, myDeathCard);
        logData = new CardTraumaCCGEvent(CCGEventType.CardDeath, GetCurrentHealth(false), myDeathCard.instanceId,
            myDeathCard.activeData.owner, embarkedPilot.instanceId, embarkedPilot.activeData.owner);
        GameState.AddCCGEventLog(logData);
        if (myDeathCard.GetTemplate().IsCombatUnit())
        {
            UnitCard unitCard2 = (UnitCard) myDeathCard;
            string xpTrigger2 = "Destroy_Pilot";
            unitCard2.CheckAndUpdateXP(xpTrigger2);
            if (unitCard2.HasPilot())
            {
                unitCard2.embarkedPilot.CheckAndUpdateXP(xpTrigger2);
            }
        }
    }

    public override void TestCardDeathState()
    {
        if (CanDiscard() && !isDead)
        {
            if (myDeathCard == null)
            {
                myDeathCard = this;
            }

            CheckForDeathEvent();
        }
    }

    public override sbyte HealDamage(CardStack stack, sbyte heal)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        sbyte currentHealth = activeEntityCardData.currentHealth;
        sbyte b = currentHealth;
        currentHealth = ((currentHealth + heal <= maxHealth) ? ((sbyte) (currentHealth + heal)) : maxHealth);
        SetCurrentHealth(currentHealth);
        return (sbyte) (currentHealth - b);
    }

    public override bool CanDiscard()
    {
        return GetCurrentHealth(false) <= 0;
    }

    public override sbyte GetCurrentHealth(bool combatLog)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        sbyte b = activeEntityCardData.currentHealth;
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
            b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
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

    public void SetCurrentHealth(sbyte health)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        activeEntityCardData.currentHealth = health;
    }

    public sbyte GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetMaxHealth(sbyte health)
    {
        maxHealth = health;
    }

    public override sbyte GetMaxModHealth()
    {
        sbyte b = maxHealth;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (b2 != 0)
            {
                b += b2;
            }
        }

        return b;
    }

    public override void CreateActiveData()
    {
        if (activeData == null)
        {
            activeData = new ActiveEntityCardData();
            activeData.Setup(this);
        }
    }

    public override void InitActiveData()
    {
        if (activeData != null)
        {
            base.InitActiveData();
        }

        EntityCardTemplate entityTemplate = (EntityCardTemplate) GetTemplate();
        maxHealth = entityTemplate.Health;
        myDeathCard = null;
        isDead = false;
        if (secrets == null)
        {
            secrets = new List<Card>();
            return;
        }

        for (int i = 0; i < secrets.Count; i++)
        {
            secrets[i].InitActiveData();
        }
    }

    protected void SetActed(sbyte action)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        activeEntityCardData.acted = (sbyte) ((byte) activeEntityCardData.acted | (byte) action);
    }

    private void ClearActed()
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        activeEntityCardData.acted = 0;
    }

    public void ClearActed(sbyte action)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) activeData;
        activeEntityCardData.acted = (sbyte) ((byte) activeEntityCardData.acted & ~(byte) action);
    }

    public override void CardDeployed(Card deployed)
    {
        base.CardDeployed(deployed);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardDeployed(deployed);
            }
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        ClearActed();
        if (secrets == null)
        {
            return;
        }

        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].NewTurn(playerIndex);
        }

        if (CanDiscard())
        {
            if (myDeathCard == null)
            {
                myDeathCard = this;
            }

            CheckForDeathEvent();
        }
    }

    public override void EndTurn(sbyte playerIndex)
    {
        base.EndTurn(playerIndex);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].EndTurn(playerIndex);
            }
        }
    }

    public override void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        base.CardMoved(card, target, region, origin);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardMoved(card, target, region, origin);
            }
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardAttacked(attacker, target);
            }
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardCounterAttacked(attacker, target);
            }
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardGainedStatus(theCard, source, statusType);
            }
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardDamaged(damagedCard, source);
            }
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardDied(deadCard, source);
            }
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
            }
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].CardDiscardEffect(playerIndex, numberOfCards);
            }
        }
    }

    public override void SecretTriggered(Card secret, Card source)
    {
        base.SecretTriggered(secret, source);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].SecretTriggered(secret, source);
            }
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].SecretDestroyed(secret, source);
            }
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].TraitEffectActivating(effect, source, target, region);
            }
        }
    }

    public override void Discard(Player[] players)
    {
        base.Discard(players);
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num].Discard(players);
            }
        }
    }
}