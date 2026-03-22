using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.Data;
using Frontline.Battle.Traits;

namespace Frontline.Battle;

public class Player
{
    [JsonInclude]
    public readonly Deck Deck;

    [JsonInclude]
    public readonly SupportDeck SupportDeck;

    [JsonInclude]
    public readonly CardCollection Hand;

    [JsonInclude]
    public readonly CardCollection Discard;

    [JsonInclude]
    public readonly GameResources Resources;

    [JsonInclude]
    public readonly CardStack Commander;

    [JsonInclude]
    public readonly List<Card> Secrets = [];

    [JsonInclude]
    public readonly string Name;

    [JsonInclude]
    public readonly int UserId;

    public bool InitialCardsSwapped { get; set; }

    public bool Surrender { get; set; }

    public bool EndTurnTraitsTriggered { get; set; }

    private readonly CcgGameState _gameState;

    public Player(CcgGameState gameState, int id, string profileName, List<Card> cards, List<Card> support,
        CommanderCard currentCommander, sbyte playerIndex, bool skipShuffle)
    {
        _gameState = gameState;
        UserId = id;
        Name = profileName;

        Deck = new Deck(cards);
        Deck.Shuffle(skipShuffle);

        SupportDeck = new SupportDeck(_gameState, support, playerIndex, skipShuffle);
        if (!skipShuffle)
        {
            SupportDeck.Shuffle(skipShuffle);
        }

        Hand = new CardCollection(_gameState.GetGameTemplate().InitialDraw, Deck, _gameState, playerIndex);
        Discard = new CardCollection(0, null, _gameState, playerIndex);
        Resources = new GameResources(_gameState.GetGameTemplate().InitialPlayerHealth);

        var commanderCard = (CommanderCard) currentCommander.GenerateAndInit(_gameState);
        commanderCard.SetPlayer(this);
        commanderCard.ActiveData.Owner = playerIndex;
        commanderCard.Setup();
        Commander = new CardStack(_gameState)
        {
            PrimaryCard = commanderCard
        };
    }

    public void ActivateCommander()
    {
        var commanderCard = (CommanderCard) Commander.PrimaryCard!;
        for (var i = 0; i < commanderCard.GetNumTraits(); i++)
        {
            var trait = commanderCard.GetTrait(i);
            if (trait.ActivateOnDeploy())
            {
                trait.Activate(commanderCard, Commander, Region.NumRegions, _gameState);
            }
        }
    }

    public Card? FindTraitActor(int cardId)
    {
        var card = Commander.FindTraitActor(cardId, Commander.PrimaryCard!.ActiveData.Owner);

        if (card == null)
        {
            card = Discard.FindCard(cardId);
        }

        if (card == null)
        {
            card = Hand.FindCard(cardId);
        }

        if (card == null)
        {
            card = Deck.FindCard(cardId);
        }

        return card;
    }

    public Card? FindCard(int cardId)
    {
        var current = SupportDeck.GetCurrent();
        if (current != null && current.InstanceId == cardId)
        {
            return current;
        }

        return Hand.FindCard(cardId);
    }

    public void NewTurn(sbyte playerIndex, sbyte drawCount)
    {
        EndTurnTraitsTriggered = false;
        var gameTemplate = _gameState.GetGameTemplate();
        Resources.NewTurn(gameTemplate);
        if (!Commander.PrimaryCard!.HasStatusEffect(ApplyStatusTraitStatusType.Stun))
        {
            SupportDeck.NewTurn(Resources.CommandAccum);
        }

        DrawFromDeck(playerIndex, drawCount, true);
    }

    public Card? DeployCard(int cardId)
    {
        var card = SupportDeck.DeployCard(cardId);
        if (card != null)
        {
            Resources.Deploy(card.GetCurrentCost());
            return card;
        }

        var card2 = Hand.RemoveCard(cardId);
        if (card2 != null)
        {
            Resources.Deploy(card2.GetCurrentCost());
            return card2;
        }

        return null;
    }

    public Card? RemoveCardForTrait(int cardId, sbyte myIndex, BaseTraitEffect effect)
    {
        Card? card;
        if (effect.Targets.Area is TargetableArea.FriendlyDiscard or TargetableArea.EnemyDiscard)
        {
            card = Discard.RemoveCard(cardId);
            if (card != null)
            {
                return card;
            }
        }
        else if (effect.Targets.Area is TargetableArea.FriendlyHand or TargetableArea.EnemyHand)
        {
            card = SupportDeck.DeployCard(cardId);
            if (card != null)
            {
                return card;
            }

            card = Hand.RemoveCard(cardId);
            if (card != null)
            {
                return card;
            }
        }
        else
        {
            card = _gameState.FindTraitActor(myIndex, cardId);
            if (card == null)
            {
                return null;
            }

            var list = _gameState.FindCardStack(card);
            if (list.Count <= 0)
            {
                return null;
            }

            var cardStack = list[0];
            card = cardStack.PrimaryCard;
            cardStack.PrimaryCard = null;
            if (card == null)
            {
                return null;
            }

            var list2 = card.GetSecrets();
            if (list2.Count > 0)
            {
                foreach (var secret in list2)
                {
                    secret.Discard(_gameState.Players);
                }

                list2.Clear();
            }

            if (card.HasPilot())
            {
                var unitCard = (UnitCard) card;
                unitCard.EmbarkedPilot!.Discard(_gameState.Players);
                unitCard.EmbarkedPilot = null;
            }

            return card;
        }

        return null;
    }

    public bool CanTriggerEndTurnTraits()
    {
        return CanSubmitActions();
    }

    public bool TriggerEndTurnTraits()
    {
        EndTurnTraitsTriggered = true;
        return true;
    }

    public bool CanEndTurn(GameTemplate gameRules, int[] cardsToDiscard)
    {
        if (Surrender)
        {
            return false;
        }

        if (!EndTurnTraitsTriggered)
        {
            return false;
        }

        if (Hand.Cards.Count - cardsToDiscard.Length > gameRules.MaxCardsInHand)
        {
            return false;
        }

        return true;
    }

    public bool EndTurn()
    {
        return true;
    }

    public void AddToDiscard(Card card)
    {
        Discard.Cards.Add(card);
    }

    public void TakeDamage(sbyte attack, sbyte bypass, Card source)
    {
        var primaryCard = Commander.PrimaryCard!;
        int health = Resources.Health;
        var totalDamage = attack + bypass;
        if (totalDamage <= 0)
        {
            return;
        }

        Resources.Health = (sbyte) (health - totalDamage);
        _gameState.CardDamaged(primaryCard, source);
        var cardDamageEvent = new CardTraumaCcgEvent(CcgEventType.CardDamage, totalDamage, source.InstanceId,
            source.ActiveData.Owner, primaryCard.InstanceId, primaryCard.ActiveData.Owner);
        _gameState.AddCcgEventLog(cardDamageEvent);
        if (Resources.Health > 0)
        {
            return;
        }

        _gameState.CardDied(primaryCard, source);
        var cardDeathEvent = new CardTraumaCcgEvent(CcgEventType.CardDeath,
            primaryCard.GetCurrentHealth(false), source.InstanceId, source.ActiveData.Owner, primaryCard.InstanceId,
            primaryCard.ActiveData.Owner);
        _gameState.AddCcgEventLog(cardDeathEvent);
        if (source.GetTemplate().IsCombatUnit())
        {
            const string xpTrigger = "Destroy_Commander";
            var unitCard = (UnitCard) source;
            unitCard.CheckAndUpdateXp(xpTrigger);
            if (unitCard.HasPilot())
            {
                unitCard.EmbarkedPilot!.CheckAndUpdateXp(xpTrigger);
            }
        }
    }

    public void DrawFromDeck(sbyte playerIndex, sbyte drawCount, bool isNewTurn)
    {
        for (var i = 0; i < drawCount; i++)
        {
            if (Deck.Count == 0)
            {
                TakeDamage(0, Resources.DrawDamage++, Commander.PrimaryCard!);
                continue;
            }

            var card = Hand.DrawFromDeck(Deck, _gameState, playerIndex);
            if (card != null)
            {
                var deckDrawEvent = new CardDrawCcgEvent(CcgEventType.DeckDraw, card.InstanceId, playerIndex,
                    card.TemplateId, card.Rank);
                _gameState.AddCcgEventLog(deckDrawEvent);
                _gameState.CardDrawn(card, true, isNewTurn);
            }
        }
    }

    public bool CanSubmitActions()
    {
        return !EndTurnTraitsTriggered && !Surrender;
    }
}