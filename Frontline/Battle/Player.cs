using Frontline.Battle.CcgEvents;
using Frontline.Game;

namespace Frontline.Battle;

public class Player
{
    public Deck deck;

    public SupportDeck supportDeck;

    public CardCollection hand;

    public CardCollection discard;

    public GameResources resources;

    public CardStack commander;

    public List<Card> secrets;

    public string name;

    public int userId;

    public bool initialCardsSwapped;

    public bool surrender;

    public bool EndTurnTraitsTriggered;

    private readonly CCG _gameState;

    public Player(CCG gameState)
    {
        _gameState = gameState;
    }

    public void Create(int id, string profileName, List<Card> cards, List<Card> support, Card currentCommander,
        GameTemplate gameTemplate, sbyte playerIndex, bool skipShuffle)
    {
        userId = id;
        deck = new Deck();
        deck.cards = cards;
        deck.Shuffle(skipShuffle);
        deck.count = (sbyte) cards.Count;
        supportDeck = new SupportDeck(_gameState);
        supportDeck.Create(support, _gameState, playerIndex, skipShuffle);
        hand = new CardCollection();
        hand.Create(gameTemplate.InitialDraw, deck, _gameState, playerIndex);
        discard = new CardCollection();
        discard.Create(0, null, _gameState, playerIndex);
        resources = new GameResources();
        resources.Create(gameTemplate.InitialPlayerHealth);
        if (currentCommander != null)
        {
            CommanderCard commanderCard = (CommanderCard) currentCommander.GenerateAndInit(_gameState);
            commanderCard.SetPlayer(this);
            commanderCard.activeData.owner = playerIndex;
            commanderCard.Setup();
            commander = new CardStack();
            commander.Create();
            commander.primaryCard = commanderCard;
        }

        secrets = new List<Card>();
        name = profileName;
    }

    public void ActivateCommander()
    {
        CommanderCard commanderCard = (CommanderCard) commander.primaryCard;
        for (int i = 0; i < commanderCard.GetNumTraits(); i++)
        {
            BaseTrait trait = commanderCard.GetTrait(i);
            if (trait.ActivateOnDeploy())
            {
                trait.Activate(commanderCard, commander, RegionEnum.NumRegions, _gameState);
            }
        }
    }

    public void Init(sbyte playerIndex)
    {
        supportDeck.Init(_gameState, playerIndex);
        hand.Init(_gameState);
        if (commander != null)
        {
            CommanderCard commanderCard =
                (CommanderCard) commander.primaryCard.GenerateAndInit(_gameState);
            commanderCard.SetPlayer(this);
            commander.primaryCard = commanderCard;
        }

        discard.Init(_gameState);
    }

    public void InitActiveData()
    {
        supportDeck.InitActiveData();
        hand.InitActiveData();
        commander.primaryCard.InitActiveData();
    }

    public Card FindTraitActor(int cardId)
    {
        Card card = null;
        if (commander != null)
        {
            card = commander.FindTraitActor(cardId, commander.primaryCard.activeData.owner);
        }

        if (card == null)
        {
            card = discard.FindCard(cardId);
        }

        if (card == null)
        {
            card = hand.FindCard(cardId);
        }

        if (card == null)
        {
            card = deck.FindCard(cardId);
        }

        return card;
    }

    public Card FindCard(int cardId)
    {
        Card current = supportDeck.GetCurrent();
        if (current != null && current.instanceId == cardId)
        {
            return current;
        }

        return hand.FindCard(cardId);
    }

    public void NewTurn(sbyte playerIndex, sbyte drawCount)
    {
        EndTurnTraitsTriggered = false;
        GameTemplate gameTemplate = _gameState.GetGameTemplate();
        resources.NewTurn(gameTemplate);
        if (!commander.primaryCard.HasStatusEffect(1))
        {
            supportDeck.NewTurn(resources.commandAccum);
        }

        DrawFromDeck(playerIndex, drawCount, true);
    }

    public Card DeployCard(int cardId)
    {
        Card card = supportDeck.DeployCard(cardId);
        if (card != null)
        {
            resources.Deploy(card.GetCurrentCost());
            return card;
        }

        Card card2 = hand.RemoveCard(cardId);
        if (card2 != null)
        {
            resources.Deploy(card2.GetCurrentCost());
            return card2;
        }

        return null;
    }

    public Card RemoveCardForTrait(int cardId, sbyte myIndex, BaseTraitEffect effect)
    {
        Card card = null;
        if (effect.targets.area == TargetableArea.FriendlyDiscard || effect.targets.area == TargetableArea.EnemyDiscard)
        {
            card = discard.RemoveCard(cardId);
            if (card != null)
            {
                return card;
            }
        }
        else if (effect.targets.area == TargetableArea.FriendlyHand || effect.targets.area == TargetableArea.EnemyHand)
        {
            card = supportDeck.DeployCard(cardId);
            if (card != null)
            {
                return card;
            }

            card = hand.RemoveCard(cardId);
            if (card != null)
            {
                return card;
            }
        }
        else
        {
            card = _gameState.FindTraitActor(myIndex, cardId);
            List<CardStack> list = _gameState.FindCardStack(card);
            card = null;
            if (list != null && list.Count > 0)
            {
                CardStack cardStack = list[0];
                card = cardStack.primaryCard;
                cardStack.primaryCard = null;
                if (card != null)
                {
                    List<Card> list2 = card.GetSecrets();
                    if (list2 != null && list2.Count > 0)
                    {
                        for (int i = 0; i < list2.Count; i++)
                        {
                            list2[i].Discard(_gameState.players);
                        }

                        list2.Clear();
                    }

                    if (card.HasPilot())
                    {
                        UnitCard unitCard = (UnitCard) card;
                        unitCard.embarkedPilot.Discard(_gameState.players);
                        unitCard.embarkedPilot = null;
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
        if (surrender)
        {
            return false;
        }

        if (!EndTurnTraitsTriggered)
        {
            return false;
        }

        if (hand.cards.Count - cardsToDiscard.Length > gameRules.MaxCardsInHand)
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
        discard.cards.Add(card);
    }

    public void TakeDamage(sbyte attack, sbyte bypass, Card source)
    {
        sbyte b = attack;
        sbyte b2 = bypass;
        Card primaryCard = commander.primaryCard;
        int num = resources.health;
        int num2 = b + b2;
        if (num2 <= 0)
        {
            return;
        }

        resources.health = (sbyte) (num - num2);
        _gameState.CardDamaged(primaryCard, source);
        CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CCGEventType.CardDamage, num2, source.instanceId,
            source.activeData.owner, primaryCard.instanceId, primaryCard.activeData.owner);
        _gameState.AddCCGEventLog(logData);
        if (resources.health > 0)
        {
            return;
        }

        _gameState.CardDied(primaryCard, source);
        CardTraumaCCGEvent logData2 = new CardTraumaCCGEvent(CCGEventType.CardDeath,
            primaryCard.GetCurrentHealth(false), source.instanceId, source.activeData.owner, primaryCard.instanceId,
            primaryCard.activeData.owner);
        _gameState.AddCCGEventLog(logData2);
        if (source.GetTemplate().IsCombatUnit())
        {
            UnitCard unitCard = (UnitCard) source;
            string xpTrigger = "Destroy_Commander";
            unitCard.CheckAndUpdateXP(xpTrigger);
            if (unitCard.HasPilot())
            {
                unitCard.embarkedPilot.CheckAndUpdateXP(xpTrigger);
            }
        }
    }

    public void DrawFromDeck(sbyte playerIndex, sbyte drawCount, bool isNewTurn)
    {
        for (int i = 0; i < drawCount; i++)
        {
            if (deck.count == 0)
            {
                TakeDamage(0, resources.drawDamage++, commander.primaryCard);
                continue;
            }

            Card card = hand.DrawFromDeck(deck, _gameState, playerIndex);
            if (card != null)
            {
                CardDrawCCGEvent logData = new CardDrawCCGEvent(CCGEventType.DeckDraw, card.instanceId, playerIndex,
                    card.templateId, card.rank);
                _gameState.AddCCGEventLog(logData);
                _gameState.CardDrawn(card, true, isNewTurn);
            }
        }
    }

    public bool CanSubmitActions()
    {
        return !EndTurnTraitsTriggered && !surrender;
    }
}