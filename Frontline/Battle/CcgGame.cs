namespace Frontline.Battle;

public class CcgGame
{
    public readonly Guid Id;

    public readonly int Player1Id;

    public readonly int Player2Id;

    public readonly VersusType VersusType;

    public int GameChangeCounter { get; private set; }

    public CcgGame(int player1Id, int player2Id, VersusType versusType)
    {
        Id = Guid.NewGuid();
        Player1Id = player1Id;
        Player2Id = player2Id;
        VersusType = versusType;
    }

    public bool IsPlayerInGame(int userId)
    {
        return Player1Id == userId || Player2Id == userId;
    }
}