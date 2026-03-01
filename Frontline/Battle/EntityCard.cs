using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class EntityCard : Card
{
    public List<Card> Secrets { get; set; }

    private sbyte _maxHealth;

    private Card? _myDeathCard;

    private bool _isDead;

    public EntityCard(CcgGameState game)
        : base(game)
    {
    }

    public EntityCard(CcgGameState game, Card other)
        : base(game, other)
    {
        Secrets = other.GetSecrets();
    }

    public EntityCard(CcgGameState game, ItemEntity itemEntity)
        : base(game, itemEntity)
    {
    }

    public override void Setup()
    {
        base.Setup();
        var entityTemplate = (EntityCardTemplate) GetTemplate();
        _maxHealth = entityTemplate.Health;
        Secrets = new List<Card>();
        _myDeathCard = null;
        _isDead = false;
    }

    public override List<Card> GetSecrets()
    {
        return Secrets;
    }

    public override void InitStackedCards()
    {
        if (Secrets != null)
        {
            for (var num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num] = Secrets[num].GenerateAndInit(GameState);
            }
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

        if (Secrets == null)
        {
            return card;
        }

        for (var i = 0; i < Secrets.Count; i++)
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

        for (var i = 0; i < Secrets.Count; i++)
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

    public override bool Deploy(CardStack stack, bool embark, Region target, CardTransitionCcgEvent? deployEvent)
    {
        if (stack.PrimaryCard == null)
        {
            SetActed(EntityActionType.AnyActionMask);
            stack.PrimaryCard = this;
            for (var i = 0; i < cardTraits.Length; i++)
            {
                var baseTrait = cardTraits[i];
                if (baseTrait != null && baseTrait.ActivateOnDeploy())
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

    public override bool HasActed(sbyte actions)
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
        if (HasActed(8))
        {
            return false;
        }

        var activationTrait = GetActivationTrait();
        if (activationTrait == null)
        {
            return false;
        }

        var primaryTargeting = activationTrait.GetPrimaryTargeting(0);
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

        var flag = false;
        var flag2 = GameState.HasInterceptBattleEffect(ActiveData.Owner);
        if (flag2)
        {
            flag = target.PrimaryCard.HasIntercept();
        }

        if (flag2 && !flag)
        {
            GameState.Logger.Debug("EntityCard.CanAttack false - target card is not intercept");
        }

        return !flag2 || flag;
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
        for (var i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].TraitType == TraitType.OneShot && !ActiveData.TraitActivated[i])
            {
                SetActed(EntityActionType.AnyButDeployMask);
                ActiveData.TraitActivated[i] = true;
                for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
                {
                    var activeTrait = ActiveData.ActiveTraits[num];
                    activeTrait.GetTraitInfo().ActivateAction(target, region, activeTrait);
                }

                cardTraits[i].Activate(this, target, region, game);
                break;
            }
        }
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        var currentHealth = activeEntityCardData.CurrentHealth;
        var b = attack;
        var b2 = bypass;
        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            var activeTrait = ActiveData.ActiveTraits[i];
            if (activeTrait.GetTraitInfo().IsDamageImmunity(false, activeTrait))
            {
                b = 0;
            }

            if (activeTrait.GetTraitInfo().IsDamageImmunity(true, activeTrait))
            {
                b2 = 0;
            }
        }

        var b3 = (sbyte) (b + b2);
        if (b3 > 0)
        {
            SetCurrentHealth((sbyte) (currentHealth - b3));
            GameState.CardDamaged(this, source);
            var logData = new CardTraumaCcgEvent(CcgEventType.CardDamage, b3, source.InstanceId,
                source.ActiveData.Owner, InstanceId, ActiveData.Owner);
            GameState.AddCcgEventLog(logData);
            if (_myDeathCard == null && CanDiscard())
            {
                _myDeathCard = source;
            }

            if (checkDeath)
            {
                CheckForDeathEvent();
            }
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
        CardStack cardStack = null;
        if (list == null || list.Count == 0)
        {
            return;
        }

        cardStack = list[0];
        for (var i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].TraitType == TraitType.LastStand && !ActiveData.TraitActivated[i])
            {
                ActiveData.TraitActivated[i] = true;
                cardTraits[i].Activate(this, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(this, _myDeathCard);
        var logData = new CardTraumaCcgEvent(CcgEventType.CardDeath, GetCurrentHealth(false),
            _myDeathCard.InstanceId, _myDeathCard.ActiveData.Owner, InstanceId, ActiveData.Owner);
        GameState.AddCcgEventLog(logData);
        if (_myDeathCard.GetTemplate().IsCombatUnit())
        {
            var unitCard = (UnitCard) _myDeathCard;
            var xpTrigger = "Destroy_" + template.Type;
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
        for (var j = 0; j < embarkedPilot.cardTraits.Length; j++)
        {
            if (embarkedPilot.cardTraits[j].TraitType == TraitType.LastStand &&
                !embarkedPilot.ActiveData.TraitActivated[j])
            {
                embarkedPilot.ActiveData.TraitActivated[j] = true;
                embarkedPilot.cardTraits[j].Activate(embarkedPilot, cardStack, traitActorRegion, GameState);
            }
        }

        GameState.CardDied(embarkedPilot, _myDeathCard);
        logData = new CardTraumaCcgEvent(CcgEventType.CardDeath, GetCurrentHealth(false), _myDeathCard.InstanceId,
            _myDeathCard.ActiveData.Owner, embarkedPilot.InstanceId, embarkedPilot.ActiveData.Owner);
        GameState.AddCcgEventLog(logData);
        if (_myDeathCard.GetTemplate().IsCombatUnit())
        {
            var unitCard2 = (UnitCard) _myDeathCard;
            var xpTrigger2 = "Destroy_Pilot";
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
        var b = currentHealth;
        currentHealth = currentHealth + heal <= _maxHealth ? (sbyte) (currentHealth + heal) : _maxHealth;
        SetCurrentHealth(currentHealth);
        return (sbyte) (currentHealth - b);
    }

    public override bool CanDiscard()
    {
        return GetCurrentHealth(false) <= 0;
    }

    public override sbyte GetCurrentHealth(bool combatLog)
    {
        var activeEntityCardData = (ActiveEntityCardData) ActiveData;
        var b = activeEntityCardData.CurrentHealth;
        List<EventLogTraitCardInfo> list = [];

        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            var activeTrait = ActiveData.ActiveTraits[i];
            var b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    var eventLogTraitCardInfo = new EventLogTraitCardInfo
                    {
                        InstanceId = activeTrait.GetTraitSource().InstanceId,
                        Owner = activeTrait.GetTraitSource().ActiveData.Owner,
                        EffectId = activeTrait.GetTraitInfo().EffectTraitId,
                        TraitId = activeTrait.GetTraitInfo().TraitParentId,
                        Data = b2
                    };
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

            GameState.AddCcgEventLog(combatBuffsCCGEvent);
        }

        return b;
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
        var b = _maxHealth;
        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            var activeTrait = ActiveData.ActiveTraits[i];
            var b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
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

        var entityTemplate = (EntityCardTemplate) GetTemplate();
        _maxHealth = entityTemplate.Health;
        _myDeathCard = null;
        _isDead = false;
        if (Secrets == null)
        {
            Secrets = new List<Card>();
            return;
        }

        for (var i = 0; i < Secrets.Count; i++)
        {
            Secrets[i].InitActiveData();
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
        if (Secrets != null)
        {
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
        if (Secrets != null)
        {
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].CardCounterAttacked(attacker, target);
            }
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        if (Secrets != null)
        {
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
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
            for (var num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num].Discard(players);
            }
        }
    }
}