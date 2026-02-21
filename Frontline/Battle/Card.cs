using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class Card : Item
{
    public ActiveCardData activeData;

    public int xp;

    public sbyte rank = 1;

    protected CardTemplate template;

    protected BaseTrait[] cardTraits;

    protected sbyte currentCost;

    protected readonly CCG GameState;

    public Card(CCG game)
    {
        GameState = game;
    }

    public Card(CCG game, Card other)
    {
        GameState = game;
        Copy(other);
    }

    public Card GenerateAndInit(CCG game)
    {
        if (templateId == 0)
        {
            return this;
        }

        CardTemplate cardTemplate = RulesetParser.GetCardTemplate(templateId, rank);
        if (cardTemplate == null)
        {
            Console.WriteLine("Invalid card (id {0:D} in player inventory for current ruleset (instance id {1:D})",
                templateId, instanceId);
            return this;
        }

        Card card = cardTemplate.GenerateCard(game, this);
        if (activeData != null)
        {
            card.activeData.owner = activeData.owner;
        }

        return card;
    }

    public Card Init()
    {
        if (templateId == 0)
        {
            return this;
        }

        template = RulesetParser.GetCardTemplate(templateId, rank);
        if (template != null)
        {
            AddTraitsFromTemplate();
            currentCost = template.Cost;
            CreateActiveData();
            InitStackedCards();
            return this;
        }

        Console.WriteLine("Trying to init an invalid Card ID {0} for rank {1}", templateId, rank);
        return null;
    }

    public virtual void InitStackedCards()
    {
    }

    public virtual Card FindTraitActor(int cardId, sbyte ownerId)
    {
        if (instanceId == cardId && activeData.owner == ownerId)
        {
            return this;
        }

        return null;
    }

    public virtual bool DoesMatchTargetingInfo(TraitTargeting info, Card source)
    {
        if (info.CheckFriendly() && activeData.owner == source.activeData.owner)
        {
            if (info.DoesMatchType(this))
            {
                if (info.scope != 0 && info.scope != TraitTargetScope.UnitStack)
                {
                    return true;
                }

                if (source.EqualsTo(this))
                {
                    return true;
                }
            }
        }
        else if (info.CheckEnemy() && activeData.owner != source.activeData.owner && info.DoesMatchType(this))
        {
            return true;
        }

        return false;
    }

    protected void AddTraitsFromTemplate()
    {
        int[] traits = template.Traits;
        int num = traits.Length;
        cardTraits = new BaseTrait[num];
        for (int i = 0; i < num; i++)
        {
            cardTraits[i] = RulesetParser.GetTraitTemplate(traits[i]);
            if (cardTraits[i] == null)
            {
                Console.WriteLine("Could Not Load Valid Card Trait {0} for card {1}", traits[i], GetTemplate().CardId);
                cardTraits[i] = new BaseTrait();
                cardTraits[i].effects = new List<BaseTraitEffect>();
            }

            foreach (var effect in cardTraits[i].effects)
            {
                effect.Init(GameState);
            }
        }
    }

    public void ResetCard()
    {
        sbyte owner = activeData.owner;
        Setup();
        activeData = null;
        CreateActiveData();
        activeData.owner = owner;
    }

    public virtual void Setup()
    {
        if (template != null)
        {
            rank = (sbyte) template.MinimumRank;
        }
    }

    public CardTemplate GetTemplate()
    {
        return template;
    }

    public virtual List<Card> GetSecrets()
    {
        return null;
    }

    public void SetTemplate(CardTemplate newTemplate)
    {
        template = newTemplate;
    }

    public virtual UnitType GetUnitType()
    {
        return UnitType.NumTypes;
    }

    public sbyte GetCurrentCost()
    {
        sbyte b = currentCost;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            if (activeTrait != null)
            {
                b += activeTrait.GetTraitInfo().GetCommandMod(activeTrait);
            }
        }

        return b;
    }

    public int GetNumTraits()
    {
        return cardTraits.Length;
    }

    public BaseTrait GetTrait(int index)
    {
        return cardTraits[index];
    }

    public BaseTrait GetActivationTrait()
    {
        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].traitType == TraitType.OneShot && !activeData.traitActivated[i])
            {
                return cardTraits[i];
            }
        }

        return null;
    }

    public virtual bool CanDeployAnywhere()
    {
        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i].CanDropAnywhere())
            {
                return true;
            }
        }

        return false;
    }

    public bool HasTrait(int traitId)
    {
        for (int i = 0; i < cardTraits.Length; i++)
        {
            BaseTrait baseTrait = cardTraits[i];
            if (baseTrait != null && baseTrait.traitId == traitId)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool HasActiveTraitEffect(int effectId)
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            if (activeTrait.traitEffectId == effectId && !activeTrait.detered)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool HasActiveSourceTrait(int traitId)
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            if (activeTrait.traitSourceId == traitId && !activeTrait.detered)
            {
                return true;
            }
        }

        return false;
    }

    public void DeactivateTrait(int traitId, Card source)
    {
        activeData.DeactivateTrait(traitId, this, source);
    }

    public virtual bool Deploy(CardStack stack, bool embark, RegionEnum target, CardTransitionCCGEvent deployEvent)
    {
        if (deployEvent != null && stack.primaryCard != null)
        {
            deployEvent.targetId = stack.primaryCard.instanceId;
            deployEvent.targetOwner = stack.primaryCard.activeData.owner;
        }

        if (template.Type == CardType.Secret)
        {
            if (stack.primaryCard != null)
            {
                stack.primaryCard.GetSecrets().Add(this);
            }
            else
            {
                GameState.players[activeData.owner].secrets.Add(this);
            }
        }

        for (int i = 0; i < cardTraits.Length; i++)
        {
            BaseTrait baseTrait = cardTraits[i];
            if (baseTrait != null && baseTrait.ActivateOnDeploy())
            {
                activeData.traitActivated[i] = true;
                baseTrait.Activate(this, stack, target, GameState);
            }
        }

        if (template.Type == CardType.BurnCard)
        {
            Discard(GameState.players);
        }

        return true;
    }

    public void OnRemovedDeter()
    {
        BaseTrait baseTrait = null;
        BaseTraitEffect baseTraitEffect = null;
        RegionEnum traitActorRegion = GameState.GetTraitActorRegion(activeData.owner, instanceId);
        for (int i = 0; i < cardTraits.Length; i++)
        {
            baseTrait = cardTraits[i];
            for (int j = 0; j < baseTrait.effects.Count; j++)
            {
                baseTraitEffect = baseTrait.effects[j];
                if (baseTraitEffect.deterable && baseTraitEffect.durationData.type == TraitDurationType.Permanent &&
                    baseTraitEffect.HasBroadTargetRange())
                {
                    baseTraitEffect.CheckGlobalApply(this, traitActorRegion, true);
                }
            }
        }
    }

    protected bool CanOverrideDeploy(RegionEnum target)
    {
        BaseTrait baseTrait = null;
        for (int i = 0; i < cardTraits.Length; i++)
        {
            baseTrait = cardTraits[i];
            for (int j = 0; j < baseTrait.effects.Count; j++)
            {
                if (baseTrait.effects[j].CanDeployOverride(target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanDeploy(RegionEnum target, TargetableArea area)
    {
        CardTemplate cardTemplate = GetTemplate();
        bool flag = cardTemplate.CanDeploy(target, activeData.owner);
        if (!flag)
        {
            flag = CanOverrideDeploy(target);
        }

        if (flag)
        {
            if (cardTemplate.Type == CardType.BurnCard || cardTemplate.Type == CardType.Secret)
            {
                for (int i = 0; i < cardTraits.Length; i++)
                {
                    BaseTrait baseTrait = cardTraits[i];
                    if (baseTrait != null && !baseTrait.CanActivate(target, activeData.owner))
                    {
                        return false;
                    }
                }

                if (cardTemplate.Type == CardType.BurnCard)
                {
                    for (int j = 0; j < cardTraits.Length; j++)
                    {
                        BaseTrait baseTrait2 = cardTraits[j];
                        if (baseTrait2.HasActiveTargets(this, null, target, GameState))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            else if (area != TargetableArea.AnyAreas || area != TargetableArea.UnitStack)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public virtual bool CanDeploy(CardStack target, RegionEnum region, bool emptyAvailable, bool embark)
    {
        CardTemplate cardTemplate = GetTemplate();
        bool flag = cardTemplate.CanDeploy(region, activeData.owner);
        if (!flag)
        {
            flag = CanOverrideDeploy(region);
        }

        if (flag)
        {
            if (cardTemplate.CanDeploy(target, emptyAvailable, embark))
            {
                if (cardTemplate.Type == CardType.BurnCard || cardTemplate.Type == CardType.Secret)
                {
                    if (cardTemplate.Type == CardType.Secret && target != null && target.primaryCard != null)
                    {
                        if (target.primaryCard.GetSecrets() == null)
                        {
                            return false;
                        }

                        if (target.primaryCard.GetSecrets().Count >= 2)
                        {
                            return false;
                        }

                        Console.WriteLine("Card.CanDeploy false - Can't add secrets to this card");
                    }

                    for (int i = 0; i < cardTraits.Length; i++)
                    {
                        BaseTrait baseTrait = cardTraits[i];
                        if (baseTrait == null || !baseTrait.CanActivate(target, region, activeData.owner))
                        {
                            Console.WriteLine("Card.CanDeploy false - Trait activation not supported");
                            return false;
                        }
                    }

                    if (cardTemplate.Type == CardType.BurnCard)
                    {
                        for (int j = 0; j < cardTraits.Length; j++)
                        {
                            BaseTrait baseTrait2 = cardTraits[j];
                            if (baseTrait2.HasActiveTargets(this, target, region, GameState))
                            {
                                return true;
                            }
                        }

                        Console.WriteLine("Card.CanDeploy false - Trait activation has no active targets");
                        return false;
                    }
                }

                return true;
            }
        }
        else
        {
            Console.WriteLine("Card.CanDeploy false - Template check failed");
        }

        return false;
    }

    public virtual bool CanActivate(Card target, RegionEnum region)
    {
        return false;
    }

    public bool CanMove(RegionEnum target)
    {
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            ActiveTrait activeTrait = activeData.activeTraits[i];
            if (!activeTrait.GetTraitInfo().CanMove(target, activeData.owner, activeTrait))
            {
                Console.WriteLine("Card.CanMove false - Move prevented by trait " + activeTrait.traitEffectId);
                return false;
            }
        }

        if (HasPilot())
        {
            EntityCard embarkedPilot = GetEmbarkedPilot();
            for (int j = 0; j < embarkedPilot.activeData.activeTraits.Count; j++)
            {
                ActiveTrait activeTrait2 = embarkedPilot.activeData.activeTraits[j];
                if (!activeTrait2.GetTraitInfo().CanMove(target, embarkedPilot.activeData.owner, activeTrait2))
                {
                    Console.WriteLine("Card.CanMove false - Move prevented by trait " + activeTrait2.traitEffectId);
                    return false;
                }
            }
        }

        if (GetTemplate().CanMove(target, activeData.owner))
        {
            return true;
        }

        Console.WriteLine("Card.CanMove false - template check failed");
        return false;
    }

    public bool CanMove(CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        if (embark && target.primaryCard != null && (!CanEmbark() || !target.primaryCard.CanEmbark()))
        {
            Console.WriteLine("Card.CanMove false - embark check failed");
            return false;
        }

        if (GetTemplate().CanMove(GameState, source, target, emptyAvailable, embark))
        {
            return true;
        }

        Console.WriteLine("Card.CanMove false -- template check failed");
        return false;
    }

    public virtual bool Move(CardStack target, RegionEnum region, RegionEnum origin, bool embark)
    {
        GameState.CardMoved(this, target, region, origin);
        return true;
    }

    public void MovedCardTraitsEvent(Card moved, CardStack target, RegionEnum region, RegionEnum origin)
    {
        BaseTraitEffect baseTraitEffect = null;
        for (int i = 0; i < cardTraits.Length; i++)
        {
            for (int j = 0; j < cardTraits[i].effects.Count; j++)
            {
                baseTraitEffect = cardTraits[i].effects[j];
                baseTraitEffect.OnCardMovedEvent(this, moved, target, region, origin);
            }
        }
    }

    public virtual bool CanEmbark()
    {
        return false;
    }

    public virtual bool HasPilot()
    {
        return false;
    }

    public virtual EntityCard GetEmbarkedPilot()
    {
        return null;
    }

    public bool CanDisembark(CardStack source)
    {
        return GetTemplate().CanDisembark(source);
    }

    public virtual bool Disembark(CardStack location, RegionEnum region)
    {
        return false;
    }

    public virtual bool HasActed()
    {
        return true;
    }

    public virtual bool HasActed(sbyte actions)
    {
        if (actions == 1)
        {
            return false;
        }

        return true;
    }

    public virtual bool HasAnyActionsAvailable()
    {
        return false;
    }

    public bool CanAttack(RegionEnum target)
    {
        return false;
    }

    public virtual bool HasAttack()
    {
        return false;
    }

    public virtual bool CanAttack(CardStack source, CardStack target)
    {
        return false;
    }

    public virtual void Attack(CardStack source, Card target)
    {
    }

    public bool HasIntercept()
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            if (activeTrait.GetTraitInfo().IsIntercept(activeTrait))
            {
                return true;
            }
        }

        if (HasPilot())
        {
            Card embarkedPilot = GetEmbarkedPilot();
            return embarkedPilot.HasIntercept();
        }

        return false;
    }

    public bool IgnoresIntercept()
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            if (activeTrait.GetTraitInfo().IgnoreIntercept(activeTrait))
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool CanCounterAttack(CardStack source, CardStack target, bool inCombat)
    {
        return false;
    }

    public virtual void CounterAttack(CardStack source, Card target)
    {
    }

    public virtual bool CanDiscard()
    {
        return false;
    }

    public virtual sbyte GetCurrentAttack(Card target, bool combatLog)
    {
        return 0;
    }

    public virtual sbyte GetCurrentBypassDefense(Card target, bool combatLog)
    {
        return 0;
    }

    public virtual sbyte GetCurrentHealth(bool combatLog)
    {
        return 0;
    }

    public virtual sbyte GetMaxModHealth()
    {
        return 0;
    }

    public virtual sbyte GetCurrentDefense(bool combatLog)
    {
        return 0;
    }

    public virtual void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
    }

    public virtual sbyte HealDamage(CardStack stack, sbyte heal)
    {
        return 0;
    }

    public virtual void CheckForDeathEvent()
    {
    }

    public virtual void TestCardDeathState()
    {
    }

    public virtual void CardDeployed(Card deployed)
    {
        BaseTraitEffect baseTraitEffect = null;
        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i] != null)
            {
                for (int j = 0; j < cardTraits[i].effects.Count; j++)
                {
                    baseTraitEffect = cardTraits[i].effects[j];
                    baseTraitEffect.CheckCardDeployed(deployed, this);
                }
            }
        }

        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeData.activeTraits[num];
            activeTrait.GetTraitInfo().CardDeployed(deployed, activeTrait);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void NewTurn(sbyte playerIndex)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].NewTurn(this, playerIndex);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }

        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i] == null)
            {
                continue;
            }

            List<BaseTraitEffect> effects = cardTraits[i].effects;
            if (effects != null)
            {
                for (int j = 0; j < effects.Count; j++)
                {
                    effects[j].OnNewTurnEvent(this, playerIndex);
                }
            }
        }
    }

    public virtual void EndTurn(sbyte playerIndex)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].EndTurn(this, playerIndex);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardMoved(card, target, region, origin);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }

        MovedCardTraitsEvent(card, target, region, origin);
    }

    public virtual void CardAttacked(Card attacker, Card target)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardAttacked(attacker, target);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void CardCounterAttacked(Card attacker, Card target)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardCounterAttacked(attacker, target);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardGainedStatus(theCard, source, statusType);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void CardDamaged(Card damagedCard, Card source)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardDamaged(damagedCard, source);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void CardDied(Card deadCard, Card source)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardDied(deadCard, source);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].CardDiscardEffect(playerIndex, numberOfCards);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void SecretTriggered(Card secret, Card source)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].SecretTriggered(secret, source);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void SecretDestroyed(Card secret, Card source)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].SecretDestroyed(secret, source);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            activeData.activeTraits[num].TraitEffectActivating(effect, source, target, region);
            if (num > activeData.activeTraits.Count)
            {
                num = activeData.activeTraits.Count;
            }
        }
    }

    public virtual void Discard(Player[] players)
    {
        activeData.DeactivateTraits();
        players[activeData.owner].AddToDiscard(this);
    }

    public bool EqualsTo(Card other)
    {
        if (other == null)
        {
            return false;
        }

        return instanceId == other.instanceId && activeData.owner == other.activeData.owner;
    }

    protected void Copy(Card other)
    {
        if (other != null)
        {
            instanceId = other.instanceId;
            templateId = other.templateId;
            template = other.template;
            xp = other.xp;
            rank = other.rank;
            activeData = other.activeData;
            cardTraits = other.cardTraits;
            currentCost = other.currentCost;
        }
    }

    public virtual void CreateActiveData()
    {
        if (activeData == null)
        {
            activeData = new ActiveCardData();
            activeData.Setup(this);
        }
    }

    public virtual void InitActiveData()
    {
        if (activeData != null)
        {
            activeData.Init(GameState, this);
        }

        if (template != null)
        {
            currentCost = template.Cost;
            rank = (sbyte) template.MinimumRank;
        }
    }

    public virtual bool HasStatusEffect(sbyte effectID)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeData.activeTraits[num];
            if (activeTrait.GetTraitInfo().IsStatusEffect(effectID, activeTrait))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCardTraitsDetered()
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeData.activeTraits[num];
            if (activeTrait == null || activeTrait.GetTraitInfo() == null)
            {
                Console.WriteLine(string.Concat("Card ", instanceId, " has missing trait data on Active Trait index ",
                    num, " !!!!!!!! ", activeTrait, " !!!!!!"));
            }
            else if (activeTrait.GetTraitInfo().IsStatusEffect(2, activeTrait) ||
                     activeTrait.GetTraitInfo().IsStatusEffect(1, activeTrait))
            {
                return true;
            }
        }

        return false;
    }

    public virtual void RemoveStatusEffect(sbyte effectID)
    {
        for (int num = activeData.activeTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = activeData.activeTraits[num];
            bool detered = activeTrait.detered;
            activeTrait.detered = false;
            if (activeTrait.GetTraitInfo().IsStatusEffect(effectID, activeTrait))
            {
                activeTrait.Deactivate(true);
            }

            activeTrait.detered = detered;
        }
    }

    public static int SortByCommandCostDescending(Card card1, Card card2)
    {
        return card2.GetCurrentCost().CompareTo(card1.GetCurrentCost());
    }

    public virtual bool IsImmobile()
    {
        return true;
    }
}