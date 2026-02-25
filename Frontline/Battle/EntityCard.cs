using Frontline.Battle.CcgEvents;
using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class EntityCard : Card
{
    public List<Card> Secrets { get; set; }

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
        Secrets = other.GetSecrets();
    }

    public EntityCard(CCG game, ItemEntity itemEntity)
        : base(game, itemEntity)
    {
    }

    public override void Setup()
    {
        base.Setup();
        EntityCardTemplate entityTemplate = (EntityCardTemplate) GetTemplate();
        maxHealth = entityTemplate.Health;
        Secrets = new List<Card>();
        myDeathCard = null;
        isDead = false;
    }

    public override List<Card> GetSecrets()
    {
        return Secrets;
    }

    public override void InitStackedCards()
    {
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num] = Secrets[num].GenerateAndInit(GameState);
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

        if (Secrets == null)
        {
            return card;
        }

        for (int i = 0; i < Secrets.Count; i++)
        {
            card = Secrets[i];
            if (card.InstanceId == cardId && card.ActiveData.Owner == ownerId)
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

        if (Secrets == null)
        {
            return true;
        }

        for (int i = 0; i < Secrets.Count; i++)
        {
            if (Secrets[i].DoesMatchTargetingInfo(info, source))
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

    public override bool Deploy(CardStack stack, bool embark, Region target, CardTransitionCCGEvent deployEvent)
    {
        if (stack.PrimaryCard == null)
        {
            SetActed(15);
            stack.PrimaryCard = this;
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
                              stack.PrimaryCard.InstanceId);
        }

        return false;
    }

    public override bool Move(CardStack target, Region region, Region origin, bool embark)
    {
        if (target.PrimaryCard != null)
        {
            if (!embark)
            {
                Console.WriteLine("MOVE FAILED - EntityCard.Move - target CardStack not empty. CID-" +
                                  target.PrimaryCard.InstanceId);
            }

            return false;
        }

        target.PrimaryCard = this;
        SetActed(14);
        ActiveData.MoveTraits(target, region, embark);
        return base.Move(target, region, origin, embark);
    }

    public override bool HasActed()
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        return activeEntityCardData.Acted != 0;
    }

    public override bool HasActed(sbyte actions)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        return ((byte) activeEntityCardData.Acted & (byte) actions) != 0;
    }

    public override bool HasAnyActionsAvailable()
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        return (((byte) activeEntityCardData.Acted & 0xE) ^ 0xE) != 0;
    }

    public override bool CanActivate(Card target, Region region)
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
        if (primaryTargeting.Targets.HasAreaTarget())
        {
            return activationTrait.CanActivate(region, ActiveData.Owner);
        }

        if (target == null)
        {
            return false;
        }

        return activationTrait.CanActivate(target, this, region, GameState);
    }

    public override bool CanAttack(CardStack source, CardStack target)
    {
        if (target.PrimaryCard == null)
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
        bool flag2 = GameState.HasInterceptBattleEffect(ActiveData.Owner);
        if (flag2)
        {
            flag = target.PrimaryCard.HasIntercept();
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
        ActiveData.AttackTraits(target);
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

    public void ActivateTrait(CardStack target, Region region, CCG game)
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].TraitType == TraitType.OneShot && !ActiveData.TraitActivated[i])
            {
                SetActed(14);
                ActiveData.TraitActivated[i] = true;
                for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
                {
                    activeTrait = ActiveData.ActiveTraits[num];
                    activeTrait.GetTraitInfo().ActivateAction(target, region, activeTrait);
                }

                cardTraits[i].Activate(this, target, region, game);
                break;
            }
        }
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        sbyte currentHealth = activeEntityCardData.CurrentHealth;
        sbyte b = attack;
        sbyte b2 = bypass;
        sbyte b3 = 0;
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
                b2 = 0;
            }
        }

        b3 = (sbyte) (b + b2);
        if (b3 > 0)
        {
            SetCurrentHealth((sbyte) (currentHealth - b3));
            GameState.CardDamaged(this, source);
            CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CcgEventType.CardDamage, b3, source.InstanceId,
                source.ActiveData.Owner, InstanceId, ActiveData.Owner);
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
        Region traitActorRegion = GameState.GetTraitActorRegion(ActiveData.Owner, InstanceId);
        List<CardStack> list = GameState.FindCardStack(this);
        CardStack cardStack = null;
        if (list == null || list.Count == 0)
        {
            return;
        }

        cardStack = list[0];
        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].TraitType == TraitType.LastStand && !ActiveData.TraitActivated[i])
            {
                ActiveData.TraitActivated[i] = true;
                cardTraits[i].Activate(this, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(this, myDeathCard);
        CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CcgEventType.CardDeath, GetCurrentHealth(false),
            myDeathCard.InstanceId, myDeathCard.ActiveData.Owner, InstanceId, ActiveData.Owner);
        GameState.AddCCGEventLog(logData);
        if (myDeathCard.GetTemplate().IsCombatUnit())
        {
            UnitCard unitCard = (UnitCard) myDeathCard;
            string xpTrigger = "Destroy_" + template.Type;
            unitCard.CheckAndUpdateXP(xpTrigger);
            if (unitCard.HasPilot())
            {
                unitCard.EmbarkedPilot.CheckAndUpdateXP(xpTrigger);
            }
        }

        if (!HasPilot())
        {
            return;
        }

        EntityCard embarkedPilot = GetEmbarkedPilot();
        for (int j = 0; j < embarkedPilot.cardTraits.Length; j++)
        {
            if (embarkedPilot.cardTraits[j].TraitType == TraitType.LastStand &&
                !embarkedPilot.ActiveData.TraitActivated[j])
            {
                embarkedPilot.ActiveData.TraitActivated[j] = true;
                embarkedPilot.cardTraits[j].Activate(embarkedPilot, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(embarkedPilot, myDeathCard);
        logData = new CardTraumaCCGEvent(CcgEventType.CardDeath, GetCurrentHealth(false), myDeathCard.InstanceId,
            myDeathCard.ActiveData.Owner, embarkedPilot.InstanceId, embarkedPilot.ActiveData.Owner);
        GameState.AddCCGEventLog(logData);
        if (myDeathCard.GetTemplate().IsCombatUnit())
        {
            UnitCard unitCard2 = (UnitCard) myDeathCard;
            string xpTrigger2 = "Destroy_Pilot";
            unitCard2.CheckAndUpdateXP(xpTrigger2);
            if (unitCard2.HasPilot())
            {
                unitCard2.EmbarkedPilot.CheckAndUpdateXP(xpTrigger2);
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
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        sbyte currentHealth = activeEntityCardData.CurrentHealth;
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
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        sbyte b = activeEntityCardData.CurrentHealth;
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
            b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
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
            CombatBuffsCCGEvent combatBuffsCCGEvent =
                new CombatBuffsCCGEvent(CcgEventType.CombatBuffsAttack, InstanceId, ActiveData.Owner, 0, 0);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public void SetCurrentHealth(sbyte health)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.CurrentHealth = health;
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
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
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
        if (ActiveData == null)
        {
            ActiveData = new ActiveEntityCardData();
            ActiveData.Setup(this);
        }
    }

    public override void InitActiveData()
    {
        if (ActiveData != null)
        {
            base.InitActiveData();
        }

        EntityCardTemplate entityTemplate = (EntityCardTemplate) GetTemplate();
        maxHealth = entityTemplate.Health;
        myDeathCard = null;
        isDead = false;
        if (Secrets == null)
        {
            Secrets = new List<Card>();
            return;
        }

        for (int i = 0; i < Secrets.Count; i++)
        {
            Secrets[i].InitActiveData();
        }
    }

    protected void SetActed(sbyte action)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.Acted = (sbyte) ((byte) activeEntityCardData.Acted | (byte) action);
    }

    private void ClearActed()
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.Acted = 0;
    }

    public void ClearActed(sbyte action)
    {
        ActiveEntityCardData activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.Acted = (sbyte) ((byte) activeEntityCardData.Acted & ~(byte) action);
    }

    public override void CardDeployed(Card deployed)
    {
        base.CardDeployed(deployed);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardDeployed(deployed);
            }
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        ClearActed();
        if (Secrets == null)
        {
            return;
        }

        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].NewTurn(playerIndex);
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
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].EndTurn(playerIndex);
            }
        }
    }

    public override void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        base.CardMoved(card, target, region, origin);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardMoved(card, target, region, origin);
            }
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardAttacked(attacker, target);
            }
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardCounterAttacked(attacker, target);
            }
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardGainedStatus(theCard, source, statusType);
            }
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardDamaged(damagedCard, source);
            }
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardDied(deadCard, source);
            }
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
            }
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardDiscardEffect(playerIndex, numberOfCards);
            }
        }
    }

    public override void SecretTriggered(Card secret, Card source)
    {
        base.SecretTriggered(secret, source);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].SecretTriggered(secret, source);
            }
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].SecretDestroyed(secret, source);
            }
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].TraitEffectActivating(effect, source, target, region);
            }
        }
    }

    public override void Discard(Player[] players)
    {
        base.Discard(players);
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].Discard(players);
            }
        }
    }
}