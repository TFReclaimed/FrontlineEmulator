using Frontline.Battle.CcgEvents;
using Frontline.Data.Entities;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class Card : Item
{
    public ActiveCardData ActiveData { get; set; }

    public int Xp { get; set; }

    public sbyte Rank { get; set; } = 1;

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

    public Card(CCG game, ItemEntity itemEntity)
    {
        GameState = game;
        InstanceId = itemEntity.ItemId;
        TemplateId = itemEntity.TemplateId;
        Xp = itemEntity.Xp;
        Rank = itemEntity.Rank;
    }

    public Card GenerateAndInit(CCG game)
    {
        if (TemplateId == 0)
        {
            return this;
        }

        CardTemplate cardTemplate = RulesetParser.GetCardTemplate(TemplateId, Rank);
        if (cardTemplate == null)
        {
            Console.WriteLine("Invalid card (id {0:D} in player inventory for current ruleset (instance id {1:D})",
                TemplateId, InstanceId);
            return this;
        }

        Card card = cardTemplate.GenerateCard(game, this);
        if (ActiveData != null)
        {
            card.ActiveData.Owner = ActiveData.Owner;
        }

        return card;
    }

    public Card Init()
    {
        if (TemplateId == 0)
        {
            return this;
        }

        template = RulesetParser.GetCardTemplate(TemplateId, Rank);
        if (template != null)
        {
            AddTraitsFromTemplate();
            currentCost = template.Cost;
            CreateActiveData();
            InitStackedCards();
            return this;
        }

        Console.WriteLine("Trying to init an invalid Card ID {0} for rank {1}", TemplateId, Rank);
        return null;
    }

    public virtual void InitStackedCards()
    {
    }

    public virtual Card FindTraitActor(int cardId, sbyte ownerId)
    {
        if (InstanceId == cardId && ActiveData.Owner == ownerId)
        {
            return this;
        }

        return null;
    }

    public virtual bool DoesMatchTargetingInfo(TraitTargeting info, Card source)
    {
        if (info.CheckFriendly() && ActiveData.Owner == source.ActiveData.Owner)
        {
            if (info.DoesMatchType(this))
            {
                if (info.Scope != 0 && info.Scope != TraitTargetScope.UnitStack)
                {
                    return true;
                }

                if (source.EqualsTo(this))
                {
                    return true;
                }
            }
        }
        else if (info.CheckEnemy() && ActiveData.Owner != source.ActiveData.Owner && info.DoesMatchType(this))
        {
            return true;
        }

        return false;
    }

    protected void AddTraitsFromTemplate()
    {
        int[] traits = template.Traits.ToArray();
        int num = traits.Length;
        cardTraits = new BaseTrait[num];
        for (int i = 0; i < num; i++)
        {
            cardTraits[i] = RulesetParser.GetTraitTemplate(traits[i]);
            if (cardTraits[i] == null)
            {
                Console.WriteLine("Could Not Load Valid Card Trait {0} for card {1}", traits[i], GetTemplate().CardId);
                cardTraits[i] = new BaseTrait();
                cardTraits[i].Effects = new List<BaseTraitEffect>();
            }

            foreach (var effect in cardTraits[i].Effects)
            {
                effect.Init(GameState);
            }
        }
    }

    public void ResetCard()
    {
        sbyte owner = ActiveData.Owner;
        Setup();
        ActiveData = null;
        CreateActiveData();
        ActiveData.Owner = owner;
    }

    public virtual void Setup()
    {
        if (template != null)
        {
            Rank = (sbyte) template.MinimumRank;
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
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
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
            if (cardTraits[i].TraitType == TraitType.OneShot && !ActiveData.TraitActivated[i])
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
            if (baseTrait != null && baseTrait.TraitId == traitId)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool HasActiveTraitEffect(int effectId)
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            if (activeTrait.TraitEffectId == effectId && !activeTrait.Detered)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool HasActiveSourceTrait(int traitId)
    {
        ActiveTrait activeTrait = null;
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            if (activeTrait.TraitSourceId == traitId && !activeTrait.Detered)
            {
                return true;
            }
        }

        return false;
    }

    public void DeactivateTrait(int traitId, Card source)
    {
        ActiveData.DeactivateTrait(traitId, this, source);
    }

    public virtual bool Deploy(CardStack stack, bool embark, Region target, CardTransitionCcgEvent deployEvent)
    {
        if (deployEvent != null && stack.PrimaryCard != null)
        {
            deployEvent.TargetId = stack.PrimaryCard.InstanceId;
            deployEvent.TargetOwner = stack.PrimaryCard.ActiveData.Owner;
        }

        if (template.Type == CardType.Secret)
        {
            if (stack.PrimaryCard != null)
            {
                stack.PrimaryCard.GetSecrets().Add(this);
            }
            else
            {
                GameState.Players[ActiveData.Owner].Secrets.Add(this);
            }
        }

        for (int i = 0; i < cardTraits.Length; i++)
        {
            BaseTrait baseTrait = cardTraits[i];
            if (baseTrait != null && baseTrait.ActivateOnDeploy())
            {
                ActiveData.TraitActivated[i] = true;
                baseTrait.Activate(this, stack, target, GameState);
            }
        }

        if (template.Type == CardType.BurnCard)
        {
            Discard(GameState.Players);
        }

        return true;
    }

    public void OnRemovedDeter()
    {
        BaseTrait baseTrait = null;
        BaseTraitEffect baseTraitEffect = null;
        Region traitActorRegion = GameState.GetTraitActorRegion(ActiveData.Owner, InstanceId);
        for (int i = 0; i < cardTraits.Length; i++)
        {
            baseTrait = cardTraits[i];
            for (int j = 0; j < baseTrait.Effects.Count; j++)
            {
                baseTraitEffect = baseTrait.Effects[j];
                if (baseTraitEffect.Deterable && baseTraitEffect.DurationData.Type == TraitDurationType.Permanent &&
                    baseTraitEffect.HasBroadTargetRange())
                {
                    baseTraitEffect.CheckGlobalApply(this, traitActorRegion, true);
                }
            }
        }
    }

    protected bool CanOverrideDeploy(Region target)
    {
        BaseTrait baseTrait = null;
        for (int i = 0; i < cardTraits.Length; i++)
        {
            baseTrait = cardTraits[i];
            for (int j = 0; j < baseTrait.Effects.Count; j++)
            {
                if (baseTrait.Effects[j].CanDeployOverride(target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanDeploy(Region target, TargetableArea area)
    {
        CardTemplate cardTemplate = GetTemplate();
        bool flag = cardTemplate.CanDeploy(target, ActiveData.Owner);
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
                    if (baseTrait != null && !baseTrait.CanActivate(target, ActiveData.Owner))
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

    public virtual bool CanDeploy(CardStack target, Region region, bool emptyAvailable, bool embark)
    {
        CardTemplate cardTemplate = GetTemplate();
        bool flag = cardTemplate.CanDeploy(region, ActiveData.Owner);
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
                    if (cardTemplate.Type == CardType.Secret && target != null && target.PrimaryCard != null)
                    {
                        if (target.PrimaryCard.GetSecrets() == null)
                        {
                            return false;
                        }

                        if (target.PrimaryCard.GetSecrets().Count >= 2)
                        {
                            return false;
                        }

                        Console.WriteLine("Card.CanDeploy false - Can't add secrets to this card");
                    }

                    for (int i = 0; i < cardTraits.Length; i++)
                    {
                        BaseTrait baseTrait = cardTraits[i];
                        if (baseTrait == null || !baseTrait.CanActivate(target, region, ActiveData.Owner))
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

    public virtual bool CanActivate(Card target, Region region)
    {
        return false;
    }

    public bool CanMove(Region target)
    {
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            ActiveTrait activeTrait = ActiveData.ActiveTraits[i];
            if (!activeTrait.GetTraitInfo().CanMove(target, ActiveData.Owner, activeTrait))
            {
                Console.WriteLine("Card.CanMove false - Move prevented by trait " + activeTrait.TraitEffectId);
                return false;
            }
        }

        if (HasPilot())
        {
            EntityCard embarkedPilot = GetEmbarkedPilot();
            for (int j = 0; j < embarkedPilot.ActiveData.ActiveTraits.Count; j++)
            {
                ActiveTrait activeTrait2 = embarkedPilot.ActiveData.ActiveTraits[j];
                if (!activeTrait2.GetTraitInfo().CanMove(target, embarkedPilot.ActiveData.Owner, activeTrait2))
                {
                    Console.WriteLine("Card.CanMove false - Move prevented by trait " + activeTrait2.TraitEffectId);
                    return false;
                }
            }
        }

        if (GetTemplate().CanMove(target, ActiveData.Owner))
        {
            return true;
        }

        Console.WriteLine("Card.CanMove false - template check failed");
        return false;
    }

    public bool CanMove(CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        if (embark && target.PrimaryCard != null && (!CanEmbark() || !target.PrimaryCard.CanEmbark()))
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

    public virtual bool Move(CardStack target, Region region, Region origin, bool embark)
    {
        GameState.CardMoved(this, target, region, origin);
        return true;
    }

    public void MovedCardTraitsEvent(Card moved, CardStack target, Region region, Region origin)
    {
        BaseTraitEffect baseTraitEffect = null;
        for (int i = 0; i < cardTraits.Length; i++)
        {
            for (int j = 0; j < cardTraits[i].Effects.Count; j++)
            {
                baseTraitEffect = cardTraits[i].Effects[j];
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

    public virtual bool Disembark(CardStack location, Region region)
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

    public bool CanAttack(Region target)
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
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
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
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
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
                for (int j = 0; j < cardTraits[i].Effects.Count; j++)
                {
                    baseTraitEffect = cardTraits[i].Effects[j];
                    baseTraitEffect.CheckCardDeployed(deployed, this);
                }
            }
        }

        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = ActiveData.ActiveTraits[num];
            activeTrait.GetTraitInfo().CardDeployed(deployed, activeTrait);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void NewTurn(sbyte playerIndex)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].NewTurn(this, playerIndex);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }

        for (int i = 0; i < cardTraits.Length; i++)
        {
            if (cardTraits[i] == null)
            {
                continue;
            }

            List<BaseTraitEffect> effects = cardTraits[i].Effects;
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
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].EndTurn(this, playerIndex);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardMoved(card, target, region, origin);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }

        MovedCardTraitsEvent(card, target, region, origin);
    }

    public virtual void CardAttacked(Card attacker, Card target)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardAttacked(attacker, target);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardCounterAttacked(Card attacker, Card target)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardCounterAttacked(attacker, target);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardGainedStatus(theCard, source, statusType);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardDamaged(Card damagedCard, Card source)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardDamaged(damagedCard, source);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardDied(Card deadCard, Card source)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardDied(deadCard, source);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardDiscardEffect(playerIndex, numberOfCards);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void SecretTriggered(Card secret, Card source)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].SecretTriggered(secret, source);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void SecretDestroyed(Card secret, Card source)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].SecretDestroyed(secret, source);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].TraitEffectActivating(effect, source, target, region);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void Discard(Player[] players)
    {
        ActiveData.DeactivateTraits();
        players[ActiveData.Owner].AddToDiscard(this);
    }

    public bool EqualsTo(Card other)
    {
        if (other == null)
        {
            return false;
        }

        return InstanceId == other.InstanceId && ActiveData.Owner == other.ActiveData.Owner;
    }

    protected void Copy(Card other)
    {
        if (other != null)
        {
            InstanceId = other.InstanceId;
            TemplateId = other.TemplateId;
            template = other.template;
            Xp = other.Xp;
            Rank = other.Rank;
            ActiveData = other.ActiveData;
            cardTraits = other.cardTraits;
            currentCost = other.currentCost;
        }
    }

    public virtual void CreateActiveData()
    {
        if (ActiveData == null)
        {
            ActiveData = new ActiveCardData();
            ActiveData.Setup(this);
        }
    }

    public virtual void InitActiveData()
    {
        if (ActiveData != null)
        {
            ActiveData.Init(GameState, this);
        }

        if (template != null)
        {
            currentCost = template.Cost;
            Rank = (sbyte) template.MinimumRank;
        }
    }

    public virtual bool HasStatusEffect(sbyte effectID)
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = ActiveData.ActiveTraits[num];
            if (activeTrait.GetTraitInfo().IsStatusEffect(effectID, activeTrait))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCardTraitsDetered()
    {
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = ActiveData.ActiveTraits[num];
            if (activeTrait == null || activeTrait.GetTraitInfo() == null)
            {
                Console.WriteLine(string.Concat("Card ", InstanceId, " has missing trait data on Active Trait index ",
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
        for (int num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveTrait activeTrait = ActiveData.ActiveTraits[num];
            bool detered = activeTrait.Detered;
            activeTrait.Detered = false;
            if (activeTrait.GetTraitInfo().IsStatusEffect(effectID, activeTrait))
            {
                activeTrait.Deactivate(true);
            }

            activeTrait.Detered = detered;
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