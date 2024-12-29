using System.Text.Json.Serialization;

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
    
    public bool IsFull => Player2Id != 0;

    public CcgGame(Guid id, int player1Id, VersusType versusType)
    {
        Id = id;
        Player1Id = player1Id;
        VersusType = versusType;
        GameEvents = [];
    }

    public void BeginGame(int player2Id)
    {
        Player2Id = player2Id;
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
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VersusType
{
    PVP_RANKED = 0,
    PVE = 1,
    Spectator = 2,
    PVP_CASUAL = 3
}