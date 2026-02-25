using Frontline.Battle.CcgEvents;
using Frontline.Game;

namespace Frontline.Battle;

public class Player
{
    public Deck Deck { get; set; }

    public SupportDeck SupportDeck { get; set; }

    public CardCollection Hand { get; set; }

    public CardCollection Discard { get; set; }

    public GameResources Resources { get; set; }

    public CardStack Commander { get; set; }

    public List<Card> Secrets { get; set; }

    public string Name { get; set; }

    public int UserId { get; set; }

    public bool InitialCardsSwapped { get; set; }

    public bool Surrender { get; set; }

    public bool EndTurnTraitsTriggered { get; set; }

    private readonly CCG _gameState;

    public Player(CCG gameState)
    {
        _gameState = gameState;
    }

    public void Create(int id, string profileName, List<Card> cards, List<Card> support, Card currentCommander,
        GameTemplate gameTemplate, sbyte playerIndex, bool skipShuffle)
    {
        UserId = id;
        Deck = new Deck();
        Deck.Cards = cards;
        Deck.Shuffle(skipShuffle);
        Deck.Count = (sbyte) cards.Count;
        SupportDeck = new SupportDeck(_gameState);
        SupportDeck.Create(support, _gameState, playerIndex, skipShuffle);
        Hand = new CardCollection();
        Hand.Create(gameTemplate.InitialDraw, Deck, _gameState, playerIndex);
        Discard = new CardCollection();
        Discard.Create(0, null, _gameState, playerIndex);
        Resources = new GameResources();
        Resources.Create(gameTemplate.InitialPlayerHealth);
        if (currentCommander != null)
        {
            CommanderCard commanderCard = (CommanderCard) currentCommander.GenerateAndInit(_gameState);
            commanderCard.SetPlayer(this);
            commanderCard.ActiveData.Owner = playerIndex;
            commanderCard.Setup();
            Commander = new CardStack();
            Commander.Create();
            Commander.PrimaryCard = commanderCard;
        }

        Secrets = new List<Card>();
        Name = profileName;
    }

    public void ActivateCommander()
    {
        CommanderCard commanderCard = (CommanderCard) Commander.PrimaryCard;
        for (int i = 0; i < commanderCard.GetNumTraits(); i++)
        {
            BaseTrait trait = commanderCard.GetTrait(i);
            if (trait.ActivateOnDeploy())
            {
                trait.Activate(commanderCard, Commander, Region.NumRegions, _gameState);
            }
        }
    }

    public void Init(sbyte playerIndex)
    {
        SupportDeck.Init(_gameState, playerIndex);
        Hand.Init(_gameState);
        if (Commander != null)
        {
            CommanderCard commanderCard =
                (CommanderCard) Commander.PrimaryCard.GenerateAndInit(_gameState);
            commanderCard.SetPlayer(this);
            Commander.PrimaryCard = commanderCard;
        }

        Discard.Init(_gameState);
    }

    public void InitActiveData()
    {
        SupportDeck.InitActiveData();
        Hand.InitActiveData();
        Commander.PrimaryCard.InitActiveData();
    }

    public Card FindTraitActor(int cardId)
    {
        Card card = null;
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

    public Card FindCard(int cardId)
    {
        Card current = SupportDeck.GetCurrent();
        if (current != null && current.InstanceId == cardId)
        {
            return current;
        }

        return Hand.FindCard(cardId);
    }

    public void NewTurn(sbyte playerIndex, sbyte drawCount)
    {
        EndTurnTraitsTriggered = false;
        GameTemplate gameTemplate = _gameState.GetGameTemplate();
        Resources.NewTurn(gameTemplate);
        if (!Commander.PrimaryCard.HasStatusEffect(1))
        {
            SupportDeck.NewTurn(Resources.CommandAccum);
        }

        DrawFromDeck(playerIndex, drawCount, true);
    }

    public Card DeployCard(int cardId)
    {
        Card card = SupportDeck.DeployCard(cardId);
        if (card != null)
        {
            Resources.Deploy(card.GetCurrentCost());
            return card;
        }

        Card card2 = Hand.RemoveCard(cardId);
        if (card2 != null)
        {
            Resources.Deploy(card2.GetCurrentCost());
            return card2;
        }

        return null;
    }

    public Card RemoveCardForTrait(int cardId, sbyte myIndex, BaseTraitEffect effect)
    {
        Card card = null;
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
            List<CardStack> list = _gameState.FindCardStack(card);
            card = null;
            if (list != null && list.Count > 0)
            {
                CardStack cardStack = list[0];
                card = cardStack.PrimaryCard;
                cardStack.PrimaryCard = null;
                if (card != null)
                {
                    List<Card> list2 = card.GetSecrets();
                    if (list2 != null && list2.Count > 0)
                    {
                        for (int i = 0; i < list2.Count; i++)
                        {
                            list2[i].Discard(_gameState.Players);
                        }

                        list2.Clear();
                    }

                    if (card.HasPilot())
                    {
                        UnitCard unitCard = (UnitCard) card;
                        unitCard.EmbarkedPilot.Discard(_gameState.Players);
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
        sbyte b = attack;
        sbyte b2 = bypass;
        Card primaryCard = Commander.PrimaryCard;
        int num = Resources.Health;
        int num2 = b + b2;
        if (num2 <= 0)
        {
            return;
        }

        Resources.Health = (sbyte) (num - num2);
        _gameState.CardDamaged(primaryCard, source);
        CardTraumaCcgEvent logData = new CardTraumaCcgEvent(CcgEventType.CardDamage, num2, source.InstanceId,
            source.ActiveData.Owner, primaryCard.InstanceId, primaryCard.ActiveData.Owner);
        _gameState.AddCCGEventLog(logData);
        if (Resources.Health > 0)
        {
            return;
        }

        _gameState.CardDied(primaryCard, source);
        CardTraumaCcgEvent logData2 = new CardTraumaCcgEvent(CcgEventType.CardDeath,
            primaryCard.GetCurrentHealth(false), source.InstanceId, source.ActiveData.Owner, primaryCard.InstanceId,
            primaryCard.ActiveData.Owner);
        _gameState.AddCCGEventLog(logData2);
        if (source.GetTemplate().IsCombatUnit())
        {
            UnitCard unitCard = (UnitCard) source;
            string xpTrigger = "Destroy_Commander";
            unitCard.CheckAndUpdateXP(xpTrigger);
            if (unitCard.HasPilot())
            {
                unitCard.EmbarkedPilot.CheckAndUpdateXP(xpTrigger);
            }
        }
    }

    public void DrawFromDeck(sbyte playerIndex, sbyte drawCount, bool isNewTurn)
    {
        for (int i = 0; i < drawCount; i++)
        {
            if (Deck.Count == 0)
            {
                TakeDamage(0, Resources.DrawDamage++, Commander.PrimaryCard);
                continue;
            }

            Card card = Hand.DrawFromDeck(Deck, _gameState, playerIndex);
            if (card != null)
            {
                CardDrawCcgEvent logData = new CardDrawCcgEvent(CcgEventType.DeckDraw, card.InstanceId, playerIndex,
                    card.TemplateId, card.Rank);
                _gameState.AddCCGEventLog(logData);
                _gameState.CardDrawn(card, true, isNewTurn);
            }
        }
    }

    public bool CanSubmitActions()
    {
        return !EndTurnTraitsTriggered && !Surrender;
    }
}