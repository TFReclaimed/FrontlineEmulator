using System.Text.Json.Serialization;
using Frontline.Data.Entities;
using Frontline.Features.Session.Inventory.GetInventory;

namespace Frontline.Game;

public class CcgGame
{
    public readonly Guid Id;
    
    public readonly int Player1Id;
    
    public readonly VersusType VersusType;

    public readonly List<GameEventParams> GameEvents;
    
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
    }

    public void BeginGame(PlayerEntity player1Entity, PlayerEntity player2Entity)
    {
        Player2Id = player2Entity.Id;
        Player1 = CreatePlayer(player1Entity);
        Player2 = CreatePlayer(player2Entity);
    }

    private Player CreatePlayer(PlayerEntity playerEntity)
    {
        return new Player
        {
            Deck = new Deck
            {
                Cards = []
            },
            SupportDeck = new SupportDeck
            {
                Cards =
                [
                    new InventoryCard
                    {
                        Type = "UnitCard",
                        InstanceId = 3,
                        TemplateId = 296
                    },
                    new InventoryCard
                    {
                        Type = "UnitCard",
                        InstanceId = 6,
                        TemplateId = 296
                    }
                ],
                Count = 2,
                CurrentSupport = 1
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
                PrimaryCard = new InventoryCard
                {
                    Type = "CommanderCard",
                    InstanceId = 2,
                    TemplateId = 283
                }
            },
            Name = playerEntity.Name,
            UserId = playerEntity.Id
        };
    }
    
    public bool IsPlayerInGame(int userId)
    {
        return Player1Id == userId || Player2Id == userId;
    }
    
    public void IncreaseChangeCounter(GameEventParams gameEvent)
    {
        GameEvents.Add(gameEvent);
        GameChangeCounter++;
        CurrentEventCount++;
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