using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Data.Entities;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

[JsonDerivedType(typeof(CommanderCard), "CommanderCard")]
[JsonDerivedType(typeof(EntityCard), "EntityCard")]
[JsonDerivedType(typeof(UnitCard), "UnitCard")]
public class Card : Item
{
    public ActiveCardData ActiveData { get; set; } = new();

    public int Xp { get; set; }

    public sbyte Rank { get; set; } = 1;

    protected CardTemplate Template;

    protected BaseTrait[] CardTraits;

    protected sbyte CurrentCost;

    protected readonly CcgGameState GameState;

    public Card(CcgGameState game, CardTemplate template)
    {
        GameState = game;
        Template = template;
        TemplateId = template.CardId;
    }

    public Card(CcgGameState game, Card other)
    {
        GameState = game;
        InstanceId = other.InstanceId;
        TemplateId = other.TemplateId;
        Template = other.Template;
        Xp = other.Xp;
        Rank = other.Rank;
        ActiveData = other.ActiveData;
        CardTraits = other.CardTraits;
        CurrentCost = other.CurrentCost;
    }

    public Card(CcgGameState game, CardTemplate template, ItemEntity itemEntity)
    {
        GameState = game;
        Template = template;
        InstanceId = itemEntity.ItemId;
        TemplateId = itemEntity.TemplateId;
        Xp = itemEntity.Xp;
        Rank = itemEntity.Rank;
    }

    public Card GenerateAndInit(CcgGameState game)
    {
        if (TemplateId == 0)
        {
            return this;
        }

        var cardTemplate = RulesetParser.GetCardTemplate(TemplateId, Rank);
        if (cardTemplate == null)
        {
            GameState.Logger.Warning("Invalid card (id {0:D} in player inventory for current ruleset (instance id {1:D})",
                TemplateId, InstanceId);
            return this;
        }

        var card = cardTemplate.GenerateCard(game, this);
        card.ActiveData.Owner = ActiveData.Owner;
        return card;
    }

    public Card? Init()
    {
        if (TemplateId == 0)
        {
            return this;
        }

        var rankedTemplate = RulesetParser.GetCardTemplate(TemplateId, Rank);
        if (rankedTemplate != null)
        {
            Template = rankedTemplate;
            AddTraitsFromTemplate();
            CurrentCost = Template.Cost;
            CreateActiveData();
            InitStackedCards();
            return this;
        }

        GameState.Logger.Warning("Trying to init an invalid Card ID {0} for rank {1}", TemplateId, Rank);
        return null;
    }

    public virtual void InitStackedCards()
    {
    }

    public virtual Card? FindTraitActor(int cardId, sbyte ownerId)
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

    private void AddTraitsFromTemplate()
    {
        var traits = Template.Traits.ToArray();
        var num = traits.Length;
        CardTraits = new BaseTrait[num];
        for (var i = 0; i < num; i++)
        {
            var traitTemplate = RulesetParser.GetTraitTemplate(traits[i]);
            if (traitTemplate == null)
            {
                GameState.Logger.Warning("Could Not Load Valid Card Trait {0} for card {1}",
                    traits[i], GetTemplate().CardId);
                traitTemplate = new BaseTrait
                {
                    Effects = []
                };
            }

            CardTraits[i] = traitTemplate;

            foreach (var effect in CardTraits[i].Effects)
            {
                effect.Init(GameState);
            }
        }
    }

    public void ResetCard()
    {
        var owner = ActiveData.Owner;
        Setup();
        CreateActiveData();
        ActiveData.Owner = owner;
    }

    public virtual void Setup()
    {
        Rank = (sbyte) Template.MinimumRank;
    }

    public CardTemplate GetTemplate()
    {
        return Template;
    }

    public virtual List<Card> GetSecrets()
    {
        return [];
    }

    public virtual UnitType GetUnitType()
    {
        return UnitType.NumTypes;
    }

    public sbyte GetCurrentCost()
    {
        var b = CurrentCost;
        for (var i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            var activeTrait = ActiveData.ActiveTraits[i];
            if (activeTrait != null)
            {
                b += activeTrait.GetTraitInfo().GetCommandMod(activeTrait);
            }
        }

        return b;
    }

    public int GetNumTraits()
    {
        return CardTraits.Length;
    }

    public BaseTrait GetTrait(int index)
    {
        return CardTraits[index];
    }

    public BaseTrait? GetActivationTrait()
    {
        for (var i = 0; i < CardTraits.Length; i++)
        {
            if (CardTraits[i].TraitType == TraitType.OneShot && !ActiveData.TraitActivated[i])
            {
                return CardTraits[i];
            }
        }

        return null;
    }

    public virtual bool HasActiveTraitEffect(int effectId)
    {
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            if (activeTrait.TraitEffectId == effectId && !activeTrait.Detered)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool HasActiveSourceTrait(int traitId)
    {
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            if (activeTrait.TraitSourceId == traitId && !activeTrait.Detered)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool Deploy(CardStack stack, bool embark, Region target, CardTransitionCcgEvent? deployEvent)
    {
        if (deployEvent != null && stack.PrimaryCard != null)
        {
            deployEvent.TargetId = stack.PrimaryCard.InstanceId;
            deployEvent.TargetOwner = stack.PrimaryCard.ActiveData.Owner;
        }

        if (Template.Type == CardType.Secret)
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

        for (var i = 0; i < CardTraits.Length; i++)
        {
            var baseTrait = CardTraits[i];
            if (baseTrait != null && baseTrait.ActivateOnDeploy())
            {
                ActiveData.TraitActivated[i] = true;
                baseTrait.Activate(this, stack, target, GameState);
            }
        }

        if (Template.Type == CardType.BurnCard)
        {
            Discard(GameState.Players);
        }

        return true;
    }

    public void OnRemovedDeter()
    {
        var traitActorRegion = GameState.GetTraitActorRegion(ActiveData.Owner, InstanceId);
        foreach (var baseTrait in CardTraits)
        {
            foreach (var baseTraitEffect in baseTrait.Effects)
            {
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
        foreach (var baseTrait in CardTraits)
        {
            foreach (var effect in baseTrait.Effects)
            {
                if (effect.CanDeployOverride(target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanDeploy(Region target, TargetableArea area)
    {
        var cardTemplate = GetTemplate();
        var flag = cardTemplate.CanDeploy(target, ActiveData.Owner);
        if (!flag)
        {
            flag = CanOverrideDeploy(target);
        }

        if (flag)
        {
            if (cardTemplate.Type == CardType.BurnCard || cardTemplate.Type == CardType.Secret)
            {
                foreach (var baseTrait in CardTraits)
                {
                    if (baseTrait != null && !baseTrait.CanActivate(target, ActiveData.Owner))
                    {
                        return false;
                    }
                }

                if (cardTemplate.Type == CardType.BurnCard)
                {
                    foreach (var baseTrait2 in CardTraits)
                    {
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
        var cardTemplate = GetTemplate();
        var flag = cardTemplate.CanDeploy(region, ActiveData.Owner);
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
                        if (target.PrimaryCard.GetSecrets().Count >= 2)
                        {
                            return false;
                        }

                        GameState.Logger.Debug("Card.CanDeploy false - Can't add secrets to this card");
                    }

                    foreach (var baseTrait in CardTraits)
                    {
                        if (baseTrait == null || !baseTrait.CanActivate(target, region, ActiveData.Owner))
                        {
                            GameState.Logger.Debug("Card.CanDeploy false - Trait activation not supported");
                            return false;
                        }
                    }

                    if (cardTemplate.Type == CardType.BurnCard)
                    {
                        foreach (var baseTrait2 in CardTraits)
                        {
                            if (baseTrait2.HasActiveTargets(this, target, region, GameState))
                            {
                                return true;
                            }
                        }

                        GameState.Logger.Debug("Card.CanDeploy false - Trait activation has no active targets");
                        return false;
                    }
                }

                return true;
            }
        }
        else
        {
            GameState.Logger.Debug("Card.CanDeploy false - Template check failed");
        }

        return false;
    }

    public virtual bool CanActivate(Card? target, Region region)
    {
        return false;
    }

    public bool CanMove(Region target)
    {
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            if (!activeTrait.GetTraitInfo().CanMove(target, ActiveData.Owner, activeTrait))
            {
                GameState.Logger.Debug("Card.CanMove false - Move prevented by trait " + activeTrait.TraitEffectId);
                return false;
            }
        }

        if (HasPilot())
        {
            var embarkedPilot = GetEmbarkedPilot()!;
            foreach (var activeTrait2 in embarkedPilot.ActiveData.ActiveTraits)
            {
                if (!activeTrait2.GetTraitInfo().CanMove(target, embarkedPilot.ActiveData.Owner, activeTrait2))
                {
                    GameState.Logger.Debug("Card.CanMove false - Move prevented by trait " + activeTrait2.TraitEffectId);
                    return false;
                }
            }
        }

        if (GetTemplate().CanMove(target, ActiveData.Owner))
        {
            return true;
        }

        GameState.Logger.Debug("Card.CanMove false - template check failed");
        return false;
    }

    public bool CanMove(CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        if (embark && target.PrimaryCard != null && (!CanEmbark() || !target.PrimaryCard.CanEmbark()))
        {
            GameState.Logger.Debug("Card.CanMove false - embark check failed");
            return false;
        }

        if (GetTemplate().CanMove(GameState, source, target, emptyAvailable, embark))
        {
            return true;
        }

        GameState.Logger.Debug("Card.CanMove false -- template check failed");
        return false;
    }

    public virtual bool Move(CardStack target, Region region, Region origin, bool embark)
    {
        GameState.CardMoved(this, target, region, origin);
        return true;
    }

    public void MovedCardTraitsEvent(Card moved, CardStack target, Region region, Region origin)
    {
        foreach (var trait in CardTraits)
        {
            foreach (var baseTraitEffect in trait.Effects)
            {
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

    public virtual EntityCard? GetEmbarkedPilot()
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

    public virtual bool HasAttack()
    {
        return false;
    }

    public virtual bool CanAttack(CardStack source, CardStack target)
    {
        return false;
    }

    public virtual void Attack(CardStack source, Card? target)
    {
    }

    public bool HasIntercept()
    {
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            if (activeTrait.GetTraitInfo().IsIntercept(activeTrait))
            {
                return true;
            }
        }

        if (HasPilot())
        {
            Card embarkedPilot = GetEmbarkedPilot()!;
            return embarkedPilot.HasIntercept();
        }

        return false;
    }

    public bool IgnoresIntercept()
    {
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
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

    public virtual sbyte GetCurrentAttack(Card? target, bool combatLog)
    {
        return 0;
    }

    public virtual sbyte GetCurrentBypassDefense(Card? target, bool combatLog)
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

    public virtual sbyte HealDamage(CardStack? stack, sbyte heal)
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
        foreach (var trait in CardTraits)
        {
            if (trait != null)
            {
                foreach (var baseTraitEffect in trait.Effects)
                {
                    baseTraitEffect.CheckCardDeployed(deployed, this);
                }
            }
        }

        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveData.ActiveTraits[num];
            activeTrait.GetTraitInfo().CardDeployed(deployed, activeTrait);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void NewTurn(sbyte playerIndex)
    {
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].NewTurn(this, playerIndex);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }

        foreach (var trait in CardTraits)
        {
            if (trait == null)
            {
                continue;
            }

            foreach (var effect in trait.Effects)
            {
                effect.OnNewTurnEvent(this, playerIndex);
            }
        }
    }

    public virtual void EndTurn(sbyte playerIndex)
    {
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            ActiveData.ActiveTraits[num].CardCounterAttacked(attacker, target);
            if (num > ActiveData.ActiveTraits.Count)
            {
                num = ActiveData.ActiveTraits.Count;
            }
        }
    }

    public virtual void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
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

    public bool EqualsTo(Card? other)
    {
        if (other == null)
        {
            return false;
        }

        return InstanceId == other.InstanceId && ActiveData.Owner == other.ActiveData.Owner;
    }

    public virtual void CreateActiveData()
    {
        ActiveData = new ActiveCardData();
        ActiveData.Setup(this);
    }

    public virtual void InitActiveData()
    {
        ActiveData.Init(GameState, this);
        CurrentCost = Template.Cost;
        Rank = (sbyte) Template.MinimumRank;
    }

    public virtual bool HasStatusEffect(ApplyStatusTraitStatusType effectId)
    {
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveData.ActiveTraits[num];
            if (activeTrait.GetTraitInfo().IsStatusEffect(effectId, activeTrait))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCardTraitsDetered()
    {
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveData.ActiveTraits[num];
            if (activeTrait == null || activeTrait.GetTraitInfo() == null)
            {
                GameState.Logger.Warning(string.Concat("Card ", InstanceId, " has missing trait data on Active Trait index ",
                    num, " !!!!!!!! ", activeTrait, " !!!!!!"));
            }
            else if (activeTrait.GetTraitInfo().IsStatusEffect(ApplyStatusTraitStatusType.Deter, activeTrait) ||
                     activeTrait.GetTraitInfo().IsStatusEffect(ApplyStatusTraitStatusType.Stun, activeTrait))
            {
                return true;
            }
        }

        return false;
    }

    public virtual void RemoveStatusEffect(ApplyStatusTraitStatusType effectId)
    {
        for (var num = ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
        {
            var activeTrait = ActiveData.ActiveTraits[num];
            var detered = activeTrait.Detered;
            activeTrait.Detered = false;
            if (activeTrait.GetTraitInfo().IsStatusEffect(effectId, activeTrait))
            {
                activeTrait.Deactivate(true);
            }

            activeTrait.Detered = detered;
        }
    }
}