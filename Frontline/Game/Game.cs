using System.Text.Json.Serialization;
using Frontline.Data.Entities;

namespace Frontline.Game;

public class CcgGame
{
    public readonly Guid Id;
    
    public readonly int Player1Id;
    
    public readonly VersusType VersusType;

    public readonly List<GameEventParams> GameEvents;

    private readonly List<CcgEventData> _ccgEventLog;

    private readonly Random _random;
    
    public Player? Player1 { get; private set; }
    
    public Player? Player2 { get; private set; }
    
    public int Player2Id { get; private set; }
    
    public int GameChangeCounter { get; private set; }
    
    public int CurrentEventCount { get; private set; }
    
    public bool SurrenderGameOver { get; private set; }
    
    public bool IsFull => Player2Id != 0;

    public CcgGame(Guid id, int player1Id, VersusType versusType)
    {
        Id = id;
        Player1Id = player1Id;
        VersusType = versusType;
        GameEvents = [];
        _ccgEventLog = [];
        _random = new Random();
    }

    public void BeginGame(PlayerEntity player1Entity, List<ItemEntity> player1Deck, List<ItemEntity> player1Support, ItemEntity player1Commander,
        PlayerEntity player2Entity, List<ItemEntity> player2Deck, List<ItemEntity> player2Support, ItemEntity player2Commander)
    {
        Player2Id = player2Entity.Id;
        Player1 = CreatePlayer(player1Entity, player1Deck, player1Support, player1Commander);
        Player1.PlayerIndex = 0;
        Player2 = CreatePlayer(player2Entity, player2Deck, player2Support, player2Commander);
        Player2.PlayerIndex = 1;
    }

    private Player CreatePlayer(PlayerEntity playerEntity, List<ItemEntity> deck, List<ItemEntity> support,
        ItemEntity commander)
    {
        var player = new Player
        {
            Deck = new Deck
            {
                Cards = deck.Select(deckItem => new GameCard
                {
                    Type = "UnitCard",
                    InstanceId = deckItem.ItemId,
                    TemplateId = deckItem.TemplateId,
                    Rank = deckItem.Rank,
                    Xp = deckItem.Xp
                }).ToList()
            },
            SupportDeck = new SupportDeck
            {
                Cards = support.Select(supportItem => new GameCard
                {
                    Type = "UnitCard",
                    InstanceId = supportItem.ItemId,
                    TemplateId = supportItem.TemplateId,
                    Rank = supportItem.Rank,
                    Xp = supportItem.Xp
                }).ToList(),
                CurrentSupport = 1 // TODO: Check if this is correct
            },
            Hand = new CardCollection
            {
                Cards = []
            },
            Discard = new CardCollection
            {
                Cards = []
            },
            Resources = new GameResources
            {
                CommandAccum = 0,
                CommandUnits = 0,
                Health = 20,
                MaxHealth = 20,
                DrawDamage = 1
            },
            Commander = new CardStack
            {
                PrimaryCard = new GameCard
                {
                    Type = "CommanderCard",
                    InstanceId = commander.ItemId,
                    TemplateId = commander.TemplateId
                }
            },
            Name = playerEntity.Name,
            UserId = playerEntity.Id
        };
        
        // TODO: get initial draw from game template
        for (var i = 0; i < 5; i++)
        {
            player.Hand.DrawFromDeck(player.Deck, player.PlayerIndex);
        }

        return player;
    }
    
    public bool IsPlayerInGame(int userId)
    {
        return Player1Id == userId || Player2Id == userId;
    }

    public Player? GetPlayer(int userId)
    {
        return Player1Id == userId ? Player1 : Player2Id == userId ? Player2 : null;
    }
    
    public void IncreaseChangeCounter(GameEventParams gameEvent)
    {
        GameEvents.Add(gameEvent);
        GameChangeCounter++;
        CurrentEventCount++;
    }

    public List<CcgEventData> GetCcgEventLog()
    {
        return _ccgEventLog;
    }
    
    public void AddCcgEvent(CcgEventData ccgEvent)
    {
        _ccgEventLog.Add(ccgEvent);
    }
    
    public void ClearCcgEventLog()
    {
        _ccgEventLog.Clear();
    }

    public void DoInitialSwap(Player player, int[] cardIdsToReshuffle, int[] deckSwapIndices)
    {
        var removedCards = new List<GameCard>();
        foreach (var cardId in cardIdsToReshuffle)
        {
            var card = player.Hand.RemoveCard(cardId);
            if (card is not null)
            {
                removedCards.Add(card);
            }
        }

        if (cardIdsToReshuffle.Length > 0)
        {
            var mulliganDrawEvent = new MulliganDrawCcgEvent
            {
                Owner = player.PlayerIndex
            };
            
            for (var i = 0; i < cardIdsToReshuffle.Length; i++)
            {
                var card = player.Hand.DrawFromDeck(player.Deck, player.PlayerIndex);
                if (card is not null)
                {
                    mulliganDrawEvent.AddDrawnCard(card);
                }
            }
            
            AddCcgEvent(mulliganDrawEvent);
        }
        
        for (var i = 0; i < removedCards.Count; i++)
        {
            deckSwapIndices[i] = _random.Next(0, player.Deck.Count + i);
            player.Deck.InsertCardAtIndex(removedCards[i], deckSwapIndices[i]);
        }

        player.InitialCardsSwapped = true;
        
        // TODO: rest of the function. force starts the game if both players have swapped i think?
    }

    public void Surrender(sbyte playerIndex)
    {
        if (playerIndex == 0)
        {
            Player1!.Surrender = true;
        }
        else
        {
            Player2!.Surrender = true;
        }
        
        SurrenderGameOver = true;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VersusType
{
    PVP_RANKED = 0,
    PVE = 1,
    Spectator = 2,
    PVP_CASUAL = 3
}