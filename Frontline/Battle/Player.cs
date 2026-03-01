using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Game;

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
        var commanderCard = (CommanderCard) Commander.PrimaryCard;
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
        Card? card = null;
        if (Commander != null)
        {
            card = Commander.FindTraitActor(cardId, Commander.PrimaryCard.ActiveData.Owner);
        }

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
        if (!Commander.PrimaryCard.HasStatusEffect(ApplyStatusTraitStatusType.Stun))
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
        if (effect.Targets.Area == TargetableArea.FriendlyDiscard || effect.Targets.Area == TargetableArea.EnemyDiscard)
        {
            card = Discard.RemoveCard(cardId);
            if (card != null)
            {
                return card;
            }
        }
        else if (effect.Targets.Area == TargetableArea.FriendlyHand || effect.Targets.Area == TargetableArea.EnemyHand)
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
            var list = _gameState.FindCardStack(card);
            if (list != null && list.Count > 0)
            {
                var cardStack = list[0];
                card = cardStack.PrimaryCard;
                cardStack.PrimaryCard = null;
                if (card != null)
                {
                    var list2 = card.GetSecrets();
                    if (list2 != null && list2.Count > 0)
                    {
                        for (var i = 0; i < list2.Count; i++)
                        {
                            list2[i].Discard(_gameState.Players);
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
            }
        }

        return null;
    }

    public bool CanTriggerEndTurnTraits(GameTemplate gameRule)
    {
        return CanSubmitActions();
    }

    public bool TriggerEndTurnTraits(GameTemplate gameRules, sbyte playerIndex)
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

    public bool EndTurn(GameTemplate gameRules, sbyte playerIndex)
    {
        return true;
    }

    public void AddToDiscard(Card card)
    {
        Discard.Cards.Add(card);
    }

    public void TakeDamage(sbyte attack, sbyte bypass, Card source)
    {
        var b = attack;
        var b2 = bypass;
        var primaryCard = Commander.PrimaryCard;
        int num = Resources.Health;
        var num2 = b + b2;
        if (num2 <= 0)
        {
            return;
        }

        Resources.Health = (sbyte) (num - num2);
        _gameState.CardDamaged(primaryCard, source);
        var logData = new CardTraumaCcgEvent(CcgEventType.CardDamage, num2, source.InstanceId,
            source.ActiveData.Owner, primaryCard.InstanceId, primaryCard.ActiveData.Owner);
        _gameState.AddCcgEventLog(logData);
        if (Resources.Health > 0)
        {
            return;
        }

        _gameState.CardDied(primaryCard, source);
        var logData2 = new CardTraumaCcgEvent(CcgEventType.CardDeath,
            primaryCard.GetCurrentHealth(false), source.InstanceId, source.ActiveData.Owner, primaryCard.InstanceId,
            primaryCard.ActiveData.Owner);
        _gameState.AddCcgEventLog(logData2);
        if (source.GetTemplate().IsCombatUnit())
        {
            var unitCard = (UnitCard) source;
            var xpTrigger = "Destroy_Commander";
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
                TakeDamage(0, Resources.DrawDamage++, Commander.PrimaryCard);
                continue;
            }

            var card = Hand.DrawFromDeck(Deck, _gameState, playerIndex);
            if (card != null)
            {
                var logData = new CardDrawCcgEvent(CcgEventType.DeckDraw, card.InstanceId, playerIndex,
                    card.TemplateId, card.Rank);
                _gameState.AddCcgEventLog(logData);
                _gameState.CardDrawn(card, true, isNewTurn);
            }
        }
    }

    public bool CanSubmitActions()
    {
        return !EndTurnTraitsTriggered && !Surrender;
    }
}