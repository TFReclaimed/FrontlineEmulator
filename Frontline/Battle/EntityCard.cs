using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class EntityCard : Card
{
    public List<Card> Secrets { get; set; } = [];

    private sbyte _maxHealth;

    private Card? _myDeathCard;

    private bool _isDead;

    public EntityCard(CcgGameState game, EntityCardTemplate template)
        : base(game, template)
    {
    }

    public EntityCard(CcgGameState game, Card other)
        : base(game, other)
    {
        Secrets = other.GetSecrets();
    }

    public EntityCard(CcgGameState game, EntityCardTemplate template, ItemEntity itemEntity)
        : base(game, template, itemEntity)
    {
    }

    public override void Setup()
    {
        base.Setup();
        var entityTemplate = (EntityCardTemplate) GetTemplate();
        _maxHealth = entityTemplate.Health;
        Secrets = [];
        _myDeathCard = null;
        _isDead = false;
    }

    public override List<Card> GetSecrets()
    {
        return Secrets;
    }

    public override void InitStackedCards()
    {
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num] = Secrets[num].GenerateAndInit(GameState);
        }

        base.InitStackedCards();
    }

    public override Card? FindTraitActor(int cardId, sbyte ownerId)
    {
        var card = base.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        foreach (var secret in Secrets)
        {
            card = secret;
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

        foreach (var secret in Secrets)
        {
            if (secret.DoesMatchTargetingInfo(info, source))
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

    public override bool Deploy(CardStack stack, bool embark, Region target, CardTransitionCcgEvent? deployEvent)
    {
        if (stack.PrimaryCard == null)
        {
            SetActed(EntityActionType.AnyActionMask);
            stack.PrimaryCard = this;
            foreach (var baseTrait in CardTraits)
            {
                if (baseTrait.ActivateOnDeploy())
                {
                    baseTrait.Activate(this, stack, target, GameState);
                }
            }

            return true;
        }

        if (!embark)
        {
            GameState.Logger.Debug("DEPLOY FAILED - EntityCard.Deploy target cardstack was not empty ID" +
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
                GameState.Logger.Warning("MOVE FAILED - EntityCard.Move - target CardStack not empty. CID-" +
                                  target.PrimaryCard.InstanceId);
            }

            return false;
        }

        target.PrimaryCard = this;
        SetActed(EntityActionType.AnyButDeployMask);
        ActiveData.MoveTraits(target, region, embark);
        return base.Move(target, region, origin, embark);
    }

    public override bool HasActed(EntityActionType actions)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        return ((byte) activeEntityCardData.Acted & (byte) actions) != 0;
    }

    public override bool HasAnyActionsAvailable()
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        return (((byte) activeEntityCardData.Acted & 0xE) ^ 0xE) != 0;
    }

    public override bool CanActivate(Card? target, Region region)
    {
        if (HasActed(EntityActionType.Activate))
        {
            return false;
        }

        var activationTrait = GetActivationTrait();
        if (activationTrait == null)
        {
            return false;
        }

        var primaryTargeting = activationTrait.GetPrimaryTargeting(0)!;
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
            GameState.Logger.Debug("EntityCard.CanAttack false - Target stack is empty");
            return false;
        }

        if (CanDiscard())
        {
            GameState.Logger.Debug("EntityCard.CanAttack false - card is dead");
            return false;
        }

        if (IgnoresIntercept())
        {
            return true;
        }

        var targetHasIntercept = false;
        var battleHasIntercept = GameState.HasInterceptBattleEffect(ActiveData.Owner);
        if (battleHasIntercept)
        {
            targetHasIntercept = target.PrimaryCard.HasIntercept();
        }

        if (battleHasIntercept && !targetHasIntercept)
        {
            GameState.Logger.Debug("EntityCard.CanAttack false - target card is not intercept");
        }

        return !battleHasIntercept || targetHasIntercept;
    }

    public override void Attack(CardStack source, Card? target)
    {
        if (target == null)
        {
            return;
        }

        SetActed(EntityActionType.AnyButDeployMask);
        ActiveData.AttackTraits(target);
        var list = GameState.FindCardStack(target);
        if (list.Count > 0)
        {
            var source2 = list[0];
            if (target.CanCounterAttack(source2, source, true))
            {
                target.CounterAttack(source2, this);
            }
        }
    }

    public void ActivateTrait(CardStack target, Region region, CcgGameState game)
    {
        for (var i = 0; i < CardTraits.Length; i++)
        {
            if (CardTraits[i].TraitType == TraitType.OneShot && !ActiveData.TraitActivated[i])
            {
                SetActed(EntityActionType.AnyButDeployMask);
                ActiveData.TraitActivated[i] = true;
                for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
                {
                    var activeTrait = ActiveData.ActiveTraits[num];
                    activeTrait.GetTraitInfo().ActivateAction(target, region, activeTrait);
                }

                CardTraits[i].Activate(this, target, region, game);
                break;
            }
        }
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        var currentHealth = activeEntityCardData.CurrentHealth;
        var attackDamage = attack;
        var bypassDamage = bypass;
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            if (activeTrait.GetTraitInfo().IsDamageImmunity(false, activeTrait))
            {
                attackDamage = 0;
            }

            if (activeTrait.GetTraitInfo().IsDamageImmunity(true, activeTrait))
            {
                bypassDamage = 0;
            }
        }

        var totalDamage = (sbyte) (attackDamage + bypassDamage);
        if (totalDamage <= 0)
        {
            return;
        }

        SetCurrentHealth((sbyte) (currentHealth - totalDamage));
        GameState.CardDamaged(this, source);
        var cardTraumaEvent = new CardTraumaCcgEvent(CcgEventType.CardDamage, totalDamage, source.InstanceId,
            source.ActiveData.Owner, InstanceId, ActiveData.Owner);
        GameState.AddCcgEventLog(cardTraumaEvent);
        if (_myDeathCard == null && CanDiscard())
        {
            _myDeathCard = source;
        }

        if (checkDeath)
        {
            CheckForDeathEvent();
        }
    }

    public override void CheckForDeathEvent()
    {
        if (_myDeathCard == null || _isDead)
        {
            return;
        }

        _isDead = true;
        var traitActorRegion = GameState.GetTraitActorRegion(ActiveData.Owner, InstanceId);
        var list = GameState.FindCardStack(this);
        if (list.Count == 0)
        {
            return;
        }

        var cardStack = list[0];
        for (var i = 0; i < CardTraits.Length; i++)
        {
            if (CardTraits[i].TraitType == TraitType.LastStand && !ActiveData.TraitActivated[i])
            {
                ActiveData.TraitActivated[i] = true;
                CardTraits[i].Activate(this, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(this, _myDeathCard);
        var cardDeathEvent = new CardTraumaCcgEvent(CcgEventType.CardDeath, GetCurrentHealth(false),
            _myDeathCard.InstanceId, _myDeathCard.ActiveData.Owner, InstanceId, ActiveData.Owner);
        GameState.AddCcgEventLog(cardDeathEvent);
        if (_myDeathCard.GetTemplate().IsCombatUnit())
        {
            var unitCard = (UnitCard) _myDeathCard;
            var xpTrigger = "Destroy_" + Template.Type;
            unitCard.CheckAndUpdateXp(xpTrigger);
            if (unitCard.HasPilot())
            {
                unitCard.EmbarkedPilot!.CheckAndUpdateXp(xpTrigger);
            }
        }

        if (!HasPilot())
        {
            return;
        }

        var embarkedPilot = GetEmbarkedPilot()!;
        for (var j = 0; j < embarkedPilot.CardTraits.Length; j++)
        {
            if (embarkedPilot.CardTraits[j].TraitType == TraitType.LastStand &&
                !embarkedPilot.ActiveData.TraitActivated[j])
            {
                embarkedPilot.ActiveData.TraitActivated[j] = true;
                embarkedPilot.CardTraits[j].Activate(embarkedPilot, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(embarkedPilot, _myDeathCard);
        cardDeathEvent = new CardTraumaCcgEvent(CcgEventType.CardDeath, GetCurrentHealth(false), _myDeathCard.InstanceId,
            _myDeathCard.ActiveData.Owner, embarkedPilot.InstanceId, embarkedPilot.ActiveData.Owner);
        GameState.AddCcgEventLog(cardDeathEvent);
        if (_myDeathCard.GetTemplate().IsCombatUnit())
        {
            const string xpTrigger2 = "Destroy_Pilot";
            var unitCard2 = (UnitCard) _myDeathCard;
            unitCard2.CheckAndUpdateXp(xpTrigger2);
            if (unitCard2.HasPilot())
            {
                unitCard2.EmbarkedPilot!.CheckAndUpdateXp(xpTrigger2);
            }
        }
    }

    public override void TestCardDeathState()
    {
        if (CanDiscard() && !_isDead)
        {
            if (_myDeathCard == null)
            {
                _myDeathCard = this;
            }

            CheckForDeathEvent();
        }
    }

    public override sbyte HealDamage(CardStack? stack, sbyte heal)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        var currentHealth = activeEntityCardData.CurrentHealth;
        var oldHealth = currentHealth;
        currentHealth = currentHealth + heal <= _maxHealth ? (sbyte) (currentHealth + heal) : _maxHealth;
        SetCurrentHealth(currentHealth);
        return (sbyte) (currentHealth - oldHealth);
    }

    public override bool CanDiscard()
    {
        return GetCurrentHealth(false) <= 0;
    }

    public override sbyte GetCurrentHealth(bool combatLog)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        var health = activeEntityCardData.CurrentHealth;
        List<EventLogTraitCardInfo> list = [];

        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            var bonus = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (bonus == 0)
            {
                continue;
            }

            if (combatLog)
            {
                var eventLogTraitCardInfo = new EventLogTraitCardInfo
                {
                    InstanceId = activeTrait.GetTraitSource().InstanceId,
                    Owner = activeTrait.GetTraitSource().ActiveData.Owner,
                    EffectId = activeTrait.GetTraitInfo().EffectTraitId,
                    TraitId = activeTrait.GetTraitInfo().TraitParentId,
                    Data = bonus
                };
                list.Add(eventLogTraitCardInfo);
            }

            health += bonus;
        }

        if (combatLog && list.Count > 0)
        {
            var count = list.Count;
            var combatBuffsEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack, InstanceId,
                ActiveData.Owner, 0, 0);
            combatBuffsEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (var j = 0; j < count; j++)
            {
                combatBuffsEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCcgEventLog(combatBuffsEvent);
        }

        return health;
    }

    public void SetCurrentHealth(sbyte health)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.CurrentHealth = health;
    }

    public sbyte GetMaxHealth()
    {
        return _maxHealth;
    }

    public void SetMaxHealth(sbyte health)
    {
        _maxHealth = health;
    }

    public override sbyte GetMaxModHealth()
    {
        var health = _maxHealth;
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            var bonus = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (bonus != 0)
            {
                health += bonus;
            }
        }

        return health;
    }

    public override void CreateActiveData()
    {
        ActiveData = new ActiveEntityCardData();
        ActiveData.Setup(this);
    }

    public override void InitActiveData()
    {
        base.InitActiveData();

        var entityTemplate = (EntityCardTemplate) GetTemplate();
        _maxHealth = entityTemplate.Health;
        _myDeathCard = null;
        _isDead = false;

        foreach (var secret in Secrets)
        {
            secret.InitActiveData();
        }
    }

    protected void SetActed(EntityActionType action)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.Acted = (sbyte) ((byte) activeEntityCardData.Acted | (byte) action);
    }

    private void ClearActed()
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.Acted = 0;
    }

    public void ClearActed(EntityActionType action)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        activeEntityCardData.Acted = (sbyte) ((byte) activeEntityCardData.Acted & ~(byte) action);
    }

    public override void CardDeployed(Card deployed)
    {
        base.CardDeployed(deployed);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDeployed(deployed);
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        ClearActed();

        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].NewTurn(playerIndex);
        }

        if (CanDiscard())
        {
            if (_myDeathCard == null)
            {
                _myDeathCard = this;
            }

            CheckForDeathEvent();
        }
    }

    public override void EndTurn(sbyte playerIndex)
    {
        base.EndTurn(playerIndex);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].EndTurn(playerIndex);
        }
    }

    public override void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        base.CardMoved(card, target, region, origin);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardMoved(card, target, region, origin);
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardAttacked(attacker, target);
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardCounterAttacked(attacker, target);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardGainedStatus(theCard, source, statusType);
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDamaged(damagedCard, source);
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDied(deadCard, source);
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public override void SecretTriggered(Card secret, Card? source)
    {
        base.SecretTriggered(secret, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].SecretTriggered(secret, source);
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].SecretDestroyed(secret, source);
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack? target, Region region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].TraitEffectActivating(effect, source, target, region);
        }
    }

    public override void Discard(Player[] players)
    {
        base.Discard(players);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].Discard(players);
        }
    }
}